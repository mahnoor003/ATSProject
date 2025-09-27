using ATSProject.Data;
using ATSProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ATSProject.Controllers
{
    public class JobsController : Controller
    {
        private readonly ATSContext _context;

        public JobsController(ATSContext context)
        {
            _context = context;
        }

        // ✅ Create Job (Recruiter)
        public IActionResult Create()
        {
            var fields = _context.CandidateFields.ToList();
            ViewBag.Fields = fields;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Job job, string selectedFields)
        {
            if (ModelState.IsValid)
            {
                job.CreatedAt = DateTime.Now;
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(selectedFields))
                {
                    var fieldIds = selectedFields.Split(',').Select(int.Parse).ToList();
                    foreach (var fieldId in fieldIds)
                    {
                        _context.JobRequiredFields.Add(new JobRequiredField
                        {
                            JobId = job.Id,   // ✅ use Id consistently
                            CandidateFieldId = fieldId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Fields = _context.CandidateFields.ToList();
            return View(job);
        }

        // ✅ Recruiter: Manage Jobs
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs.ToListAsync();
            return View(jobs);
        }

        // ✅ Candidate: Job Listings (Apply Page jesa)
        public IActionResult Listings()
        {
            var jobs = _context.Jobs.ToList();
            return View(jobs);
        }

        // ✅ Recruiter: Applicants Dashboard (Job Cards jese Candidate Listing)
        public IActionResult ApplicantsDashboard()
        {
            var jobs = _context.Jobs.ToList();
            return View(jobs);
        }

        // ✅ Recruiter: View Applicants for Specific Job
        public IActionResult ViewApplicants(int jobId)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                return NotFound();

            ViewBag.JobTitle = job.JobTitle;

            // Agar tum Applications wali table use karte ho
            var applicants = _context.CandidateApplications
                .Where(a => a.JobId == jobId)
                .Include(a => a.CandidateResponses)
                .ToList();

            return View(applicants);
        }

        // ✅ Recruiter: Old Applicants method (agar CandidateApplications table bhi hai)
        public IActionResult Applicants(int jobId)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                return NotFound();

            ViewBag.JobTitle = job.JobTitle;

            var applications = _context.CandidateApplications
                .Where(a => a.JobId == jobId)
                .Include(a => a.CandidateResponses)
                    .ThenInclude(r => r.CandidateField)
                .ToList();

            return View(applications);
        }
    }
}
