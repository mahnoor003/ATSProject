using ATSProject.Data;
using ATSProject.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System.Text.RegularExpressions;

namespace ATSProject.Services
{
    public class EmailService
    {
        private readonly ATSContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly CvParsingService _cvParser;

        private readonly GmailService _gmailService;
        private readonly SheetsService _sheetsService;

        private CancellationTokenSource? _cts; // 🔄 NEW
        private readonly string _spreadsheetId = "14jfGk5gleKGAK7-n8wVbELqz7g57yexWjlpCrbty7ME";

        public EmailService(ATSContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _cvParser = new CvParsingService();

            // ---- OAuth paths
            string credDir = Path.Combine(Directory.GetCurrentDirectory(), "Credentials");
            Directory.CreateDirectory(credDir);

            string credentialsPath = Path.Combine(credDir, "credentials.json");
            if (!File.Exists(credentialsPath))
                throw new FileNotFoundException($"Missing Google OAuth file at {credentialsPath}");

            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { GmailService.Scope.GmailModify, SheetsService.Scope.Spreadsheets },
                "user",
                CancellationToken.None,
                new FileDataStore(credDir, true)
            ).Result;

            _gmailService = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ATS Project",
            });

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "ATS Project",
            });
        }

        // 🔄 NEW: start background polling
        public void StartPolling(int minutes = 1)
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => PollLoopAsync(minutes, _cts.Token));
        }

        // 🔄 NEW: stop background polling
        public void StopPolling()
        {
            _cts?.Cancel();
        }

        // 🔄 NEW: loop that runs FetchEmailsAsync()
        private async Task PollLoopAsync(int minutes, CancellationToken token)
        {
            Console.WriteLine($"📡 Email polling started (every {minutes} min).");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await FetchEmailsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error in polling: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(minutes), token);
            }

            Console.WriteLine("📡 Email polling stopped.");
        }

public async Task FetchEmailsTestAsync()
        {
            var request = _gmailService.Users.Messages.List("me");
            request.Q = "is:unread";
            request.MaxResults = 10;

            var response = await request.ExecuteAsync();
            if (response.Messages == null || response.Messages.Count == 0)
            {
                Console.WriteLine("📭 No unread emails found.");
                return;
            }

            foreach (var m in response.Messages)
            {
                var email = await _gmailService.Users.Messages.Get("me", m.Id).ExecuteAsync();
                string subject = GetHeader(email, "Subject") ?? "(no subject)";
                string from = GetHeader(email, "From") ?? "(unknown)";
                string date = GetHeader(email, "Date") ?? "(no date)";

                Console.WriteLine("📧 Email Found:");
                Console.WriteLine($"   From: {from}");
                Console.WriteLine($"   Subject: {subject}");
                Console.WriteLine($"   Date: {date}");
                Console.WriteLine("--------------------------------");
            }
        }

        // --- PRODUCTION: fetch, download CVs, parse, save DB + Sheets, mark read
        public async Task FetchEmailsAsync()
        {
            var listRequest = _gmailService.Users.Messages.List("me");
            listRequest.Q = "is:unread has:attachment";
            listRequest.MaxResults = 50;

            var listResponse = await listRequest.ExecuteAsync();
            if (listResponse.Messages == null || listResponse.Messages.Count == 0)
                return;

            foreach (var item in listResponse.Messages)
            {
                // ✅ Skip if already processed
                if (_context.Candidates.Any(c => c.GmailMessageId == item.Id))
                    continue;

                var msg = await _gmailService.Users.Messages.Get("me", item.Id).ExecuteAsync();

                string rawFrom = GetHeader(msg, "From") ?? string.Empty;
                string senderEmail = ExtractEmail(rawFrom);
                if (string.IsNullOrWhiteSpace(senderEmail))
                    continue;

                // ✅ Extract Gmail received time (local time)
                DateTime receivedTime = msg.InternalDate.HasValue
                    ? DateTimeOffset.FromUnixTimeMilliseconds((long)msg.InternalDate.Value).UtcDateTime.ToLocalTime()
                    : DateTime.UtcNow;

                string subject = GetHeader(msg, "Subject") ?? "Not Specified";

                var candidate = new Candidate
                {
                    Email = senderEmail,
                    Name = string.Empty,
                    Phone = string.Empty,
                    RoleApplied = subject,   // ✅ Save subject as role applied
                    DateApplied = receivedTime,
                    Source = "Email",
                    UniqueId = Guid.NewGuid().ToString(),
                    GmailMessageId = item.Id
                };

                _context.Candidates.Add(candidate);

                // --- Download attachments ---
                var attachments = new List<(string FileName, byte[] Data)>();
                await CollectAttachmentsAsync(msg.Payload, attachments, msg.Id);

                foreach (var (FileName, Data) in attachments)
                {
                    var ext = Path.GetExtension(FileName).ToLowerInvariant();
                    if (ext != ".pdf" && ext != ".docx")
                        continue;

                    string uploadsFolder = Path.Combine(
                        _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                        "uploads"
                    );
                    Directory.CreateDirectory(uploadsFolder);

                    string safeName = $"{Guid.NewGuid()}_{Sanitize(FileName)}";
                    string savePath = Path.Combine(uploadsFolder, safeName);
                    await File.WriteAllBytesAsync(savePath, Data);

                    candidate.CvFilePath = "/uploads/" + safeName;

                    _cvParser.ParseCandidateCv(candidate, savePath);
                }

                await _context.SaveChangesAsync();
                await AppendToSheet(candidate);

                // ✅ Mark as read
                var mods = new ModifyMessageRequest { RemoveLabelIds = new[] { "UNREAD" } };
                await _gmailService.Users.Messages.Modify(mods, "me", msg.Id).ExecuteAsync();
            }
        }

        // --- Helpers ----------------------------------------------------------

        private static string? GetHeader(Message msg, string name) =>
            msg?.Payload?.Headers?.FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;

        private static string ExtractEmail(string fromHeader)
        {
            // Examples:
            //   "John Doe <john@example.com>"
            //   "john@example.com"
            var m = Regex.Match(fromHeader, @"<([^>]+)>"); // angle brackets
            if (m.Success) return m.Groups[1].Value.Trim();

            // fallback: find email in the string
            m = Regex.Match(fromHeader, @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}");
            return m.Success ? m.Value.Trim() : fromHeader.Trim();
        }

        private async Task CollectAttachmentsAsync(MessagePart? part, List<(string FileName, byte[] Data)> list, string messageId)
        {
            if (part == null) return;

            // If this part itself is an attachment
            if (!string.IsNullOrEmpty(part.Filename))
            {
                if (part.Body != null)
                {
                    if (!string.IsNullOrEmpty(part.Body.AttachmentId))
                    {
                        // Attachment stored separately
                        var attach = await _gmailService.Users.Messages.Attachments
                            .Get("me", messageId, part.Body.AttachmentId)
                            .ExecuteAsync();

                        var bytes = Base64UrlDecode(attach.Data);
                        list.Add((part.Filename, bytes));
                    }
                    else if (!string.IsNullOrEmpty(part.Body.Data))
                    {
                        // Inline base64 data
                        var bytes = Base64UrlDecode(part.Body.Data);
                        list.Add((part.Filename, bytes));
                    }
                }
            }

            // Recurse into child parts
            if (part.Parts != null)
            {
                foreach (var p in part.Parts)
                    await CollectAttachmentsAsync(p, list, messageId);
            }
        }


        // Overload to kick off recursion with messageId available
        private void CollectAttachments(MessagePart? root, List<(string FileName, byte[] Data)> list)
        {
            if (root == null) return;
            // Top-level needs messageId to fetch detached attachments.
            // Climb via the caller: we only have it in the parent scope, so we re-fetch with it:
            // Workaround: we call a wrapper that captures it when needed.
            // For simplicity, we’ll inject it per child call from the parent loop:
            // -> Instead, we call a different overload during per-email processing:
        }

        private static byte[] Base64UrlDecode(string data)
        {
            // Gmail uses base64url ( - _ ) instead of ( + / )
            string s = data.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }

        private static string Sanitize(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');
            return fileName;
        }

        private async Task AppendToSheet(Candidate c)
        {
            var range = "Sheet1!A:L"; // 🆕 one more column

            var values = new List<IList<object>>
    {
        new List<object>
        {
            c.Id,
            c.Name ?? string.Empty,
            c.Email ?? string.Empty,
            c.Phone ?? string.Empty,
            c.RoleApplied ?? string.Empty,
            c.Skills ?? string.Empty,
            c.Experience ?? string.Empty,
            c.Education ?? string.Empty,   // 🆕 added here
            c.DateApplied.ToString("yyyy-MM-dd HH:mm"),
            c.Source ?? string.Empty,
            c.CvFilePath ?? string.Empty,
            c.UniqueId ?? string.Empty
        }
    };

            var valueRange = new ValueRange { Values = values };
            var append = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            append.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            await append.ExecuteAsync();
        }
    }
}
