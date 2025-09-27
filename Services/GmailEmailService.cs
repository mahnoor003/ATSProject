using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;
using MimeKit;

namespace ATSProject.Services
{
    public class GmailEmailService
    {
        private readonly GmailService _gmailService;

        public GmailEmailService()
        {
            string credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "Credentials", "credentials.json");
            string tokenPath = Path.Combine(Directory.GetCurrentDirectory(), "Credentials", "token.json");

            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { GmailService.Scope.GmailReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true)
            ).Result;

            _gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ATS Project",
            });
        }

        public IList<Message> GetUnreadEmails()
        {
            var request = _gmailService.Users.Messages.List("me");
            request.Q = "is:unread has:attachment"; // unread with attachments
            var response = request.Execute();
            return response.Messages ?? new List<Message>();
        }

        public MimeMessage GetEmailContent(string messageId)
        {
            var message = _gmailService.Users.Messages.Get("me", messageId).Execute();
            var raw = message.Raw;
            var data = Convert.FromBase64String(raw.Replace('-', '+').Replace('_', '/'));
            return MimeMessage.Load(new MemoryStream(data));
        }
    }
}
