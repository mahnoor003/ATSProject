using ATSProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ATSProject.Data;


namespace ATSProject.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly ATSContext _context;
        private readonly IWebHostEnvironment _env;

        public ApplicationsController(ATSContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Applications/Apply?jobId=1  (unchanged)
        public IActionResult Apply(int jobId)
        {
            var job = _context.Jobs
                              .Include(j => j.JobRequiredFields!)
                                .ThenInclude(jrf => jrf.CandidateField)
                              .FirstOrDefault(j => j.Id == jobId);

            if (job == null) return NotFound();
            return View(job);
        }

        // POST: Submit application
        [HttpPost]
        public async Task<IActionResult> Apply(int jobId, IFormCollection form)
        {
            // load required fields and candidatefield metadata
            var jobFields = _context.JobRequiredFields
                                    .Include(jrf => jrf.CandidateField)
                                    .Where(jrf => jrf.JobId == jobId)
                                    .ToList();

            // --- 1) collect responses (including uploaded files) ---
            var responsesDict = new Dictionary<int, string?>();

            // Process file inputs from Request.Form.Files (names: field_<id>)
            foreach (var jf in jobFields)
            {
                var fieldId = jf.CandidateFieldId;
                var cf = jf.CandidateField;
                if (cf == null) continue;

                if (string.Equals(cf.FieldType, "File", StringComparison.OrdinalIgnoreCase))
                {
                    // find uploaded file with name 'field_<id>'
                    var uploaded = Request.Form.Files.FirstOrDefault(f => f.Name == $"field_{fieldId}");
                    if (uploaded != null && uploaded.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(uploaded.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await uploaded.CopyToAsync(fs);
                        }
                        var relativePath = $"/uploads/{uniqueFileName}";
                        responsesDict[fieldId] = relativePath; // store path as response
                    }
                    else
                    {
                        responsesDict[fieldId] = null;
                    }
                }
                else
                {
                    var val = form[$"field_{fieldId}"].ToString();
                    responsesDict[fieldId] = string.IsNullOrWhiteSpace(val) ? null : val;
                }
            }

            // --- 2) Try to pick candidate's name/email if present among selected fields ---
            string? candidateName = null;
            string? candidateEmail = null;
            // heuristics: look for standard field names (adjust to match your CandidateField rows)
            var nameCF = jobFields.Select(j => j.CandidateField)
                                  .FirstOrDefault(cf => cf != null && (cf.FieldName?.ToLower().Contains("full name") == true || cf.FieldName?.ToLower().Contains("name") == true));
            var emailCF = jobFields.Select(j => j.CandidateField)
                                   .FirstOrDefault(cf => cf != null && (cf.FieldName?.ToLower().Contains("email") == true));

            if (nameCF != null && responsesDict.TryGetValue(nameCF.Id, out var nVal)) candidateName = nVal;
            if (emailCF != null && responsesDict.TryGetValue(emailCF.Id, out var eVal)) candidateEmail = eVal;

            // also try to find a resume/path (file field)
            string? resumePath = null;
            var resumeCF = jobFields.Select(j => j.CandidateField)
                                    .FirstOrDefault(cf => cf != null && (cf.FieldName?.ToLower().Contains("resume") == true || cf.FieldName?.ToLower().Contains("cv") == true));
            if (resumeCF != null && responsesDict.TryGetValue(resumeCF.Id, out var rVal)) resumePath = rVal;

            // --- 3) Create CandidateApplication record ---
            var application = new CandidateApplication
            {
                JobId = jobId,
                FullName = candidateName,
                Email = candidateEmail,
                ResumePath = resumePath,
                SubmittedAt = DateTime.UtcNow
            };

            _context.CandidateApplications.Add(application);
            await _context.SaveChangesAsync(); // get application.Id

            // --- 4) Save CandidateResponse rows linked to application ---
            foreach (var kv in responsesDict)
            {
                var resp = new CandidateResponse
                {
                    JobId = jobId,
                    CandidateFieldId = kv.Key,
                    Response = kv.Value,                // ✅ matches DB
                    CandidateApplicationId = application.Id   // ✅ FK column
                };

                _context.CandidateResponses.Add(resp);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("ThankYou");
        }

        public IActionResult ThankYou()
        {
            return View();
        }

public IActionResult SelectJob()
        {
            var jobs = _context.Jobs.ToList();
            ViewBag.Jobs = new SelectList(jobs, "Id", "JobTitle");
            return View();
        }

        [HttpPost]
        public IActionResult SelectJob(int jobId)
        {
            return RedirectToAction("Apply", new { jobId });
        }

        public IActionResult Applicants(int jobId)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                return NotFound();

            ViewBag.JobTitle = job.JobTitle;

            // Load applications with their responses and fields
            var applications = _context.CandidateApplications
                .Where(a => a.JobId == jobId)
                .Include(a => a.CandidateResponses)
                    .ThenInclude(r => r.CandidateField)
                .ToList();

            return View(applications);
        }
        public IActionResult ApplicantsDashboard()
        {
            var jobs = _context.Jobs
                               .Include(j => j.CandidateApplications)
                               .ToList();

            return View(jobs);
        }


    }
}
