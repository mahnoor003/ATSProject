using ATSProject.Data;
using ATSProject.Models;
using ATSProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using Google.Apis.Util.Store;

namespace ATSProject.Controllers
{
    public class CandidatesController : Controller
    {
        private readonly ATSContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly CvParsingService _cvParser;
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId = "14jfGk5gleKGAK7-n8wVbELqz7g57yexWjlpCrbty7ME"; // 🔹 Replace with your sheet ID

        public CandidatesController(ATSContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _cvParser = new CvParsingService();

            string credDir = Path.Combine(Directory.GetCurrentDirectory(), "Credentials");
            Directory.CreateDirectory(credDir);

            string credentialsPath = Path.Combine(credDir, "credentials.json");
            if (!System.IO.File.Exists(credentialsPath))
                throw new FileNotFoundException($"Missing Google OAuth file at {credentialsPath}");

            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { SheetsService.Scope.Spreadsheets },
                "user",
                CancellationToken.None,
                new FileDataStore(credDir, true)
            ).Result;

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ATS Project",
            });

        }

        // GET: Candidates/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Candidate
            {
                Name = string.Empty,
                Email = string.Empty,
                Phone = string.Empty,
                RoleApplied = string.Empty
            });
        }

        // POST: Candidates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Candidate candidate, IFormFile cvFile)
        {
            Console.WriteLine("➡️ POST Create called");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("⚠️ Model validation failed:");
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine("   Error: " + e.ErrorMessage);

                TempData["Message"] = "⚠️ Please fill all required fields.";
                return View(candidate); // stay on form to show validation messages
            }

            // ✅ Handle CV Upload
            if (cvFile != null && cvFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(cvFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await cvFile.CopyToAsync(fs);
                    }

                    var relativePath = "/uploads/" + uniqueFileName;
                    candidate.CvFilePath = relativePath;

                    // also create full URL for Google Sheets
                    var fullUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";


                    Console.WriteLine($"✅ File uploaded: {candidate.CvFilePath}");

                    // ✅ Parse CV
                    _cvParser.ParseCandidateCv(candidate, filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error during CV upload/parse: " + ex.Message);
                    TempData["Message"] = "❌ Error while processing CV: " + ex.Message;
                    return View(candidate);
                }
            }
            else
            {
                Console.WriteLine("⚠️ No CV file uploaded.");
            }

            candidate.DateApplied = DateTime.Now;
            candidate.Source = "Form";
            candidate.UniqueId = Guid.NewGuid().ToString();

            // ✅ Prevent duplicate applications
            bool alreadyExists = await _context.Candidates
     .AnyAsync(c => c.Email == candidate.Email && c.RoleApplied == candidate.RoleApplied);

            if (alreadyExists)
            {
                Console.WriteLine("⚠️ Duplicate email detected: " + candidate.Email);
                TempData["Message"] = "⚠️ Duplicate application detected!";
                return RedirectToAction(nameof(Create));
            }

            try
            {
                // ✅ Save to Database
                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Candidate saved to DB with ID " + candidate.Id);

                // ✅ Save to Google Sheet
                await SaveToGoogleSheet(candidate);
                Console.WriteLine("✅ Candidate appended to Google Sheet");

                TempData["Message"] = "✅ Application submitted successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error while saving: " + ex);
                TempData["Message"] = "❌ Error while saving: " + ex.Message;
            }

            // 🔹 Always redirect after POST (PRG pattern)
            return RedirectToAction(nameof(Create));
        }
 

        // GET: Candidates/Index (List of candidates)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var candidates = await _context.Candidates
                .OrderByDescending(c => c.DateApplied)
                .ToListAsync();

            return View(candidates);
        }

        // 🔹 Helper: Write data into Google Sheet
        // 🔹 Helper: Write data into Google Sheet
        // 🔹 Helper: Write data into Google Sheet
        private async Task SaveToGoogleSheet(Candidate candidate)
        {
            var range = "Sheet1!A:K";

            // build full URL for sheet
            var fullUrl = $"{Request.Scheme}://{Request.Host}{candidate.CvFilePath}";

            var values = new List<IList<object>>
    {
        new List<object>
        {
            candidate.Id,
            candidate.Name,
            candidate.Email,
            candidate.Phone,
            candidate.RoleApplied,
            candidate.Skills ?? string.Empty,
            candidate.Experience ?? string.Empty,
            candidate.DateApplied.ToString("yyyy-MM-dd HH:mm"),
            candidate.Source ?? string.Empty,
            fullUrl,   // ✅ put full link in Sheet
            candidate.UniqueId ?? string.Empty
        }
    };

            var valueRange = new ValueRange { Values = values };

            var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            await appendRequest.ExecuteAsync();
        }

    }
}
