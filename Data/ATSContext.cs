using Microsoft.EntityFrameworkCore;
using ATSProject.Models;

namespace ATSProject.Data
{

    public class ATSContext : DbContext
    {
        public ATSContext(DbContextOptions<ATSContext> options) : base(options) { }

        public DbSet<Candidate> Candidates { get; set; }

        public DbSet<CandidateField> CandidateFields { get; set; }
        public DbSet<CandidateApplication> CandidateApplications { get; set; }
        public DbSet<CandidateResponse> CandidateResponses { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobRequiredField> JobRequiredFields { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CandidateApplication → Job
            modelBuilder.Entity<CandidateApplication>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // CandidateResponse → CandidateApplication
            modelBuilder.Entity<CandidateResponse>()
                .HasOne(r => r.Application)
                .WithMany(a => a.CandidateResponses)
                .HasForeignKey(r => r.CandidateApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // CandidateResponse → Job
            modelBuilder.Entity<CandidateResponse>()
                .HasOne(r => r.Job)
                .WithMany(j => j.CandidateResponses)
                .HasForeignKey(r => r.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // CandidateResponse → CandidateField
            modelBuilder.Entity<CandidateResponse>()
                .HasOne(r => r.CandidateField)
                .WithMany()
                .HasForeignKey(r => r.CandidateFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobRequiredField → Job
            modelBuilder.Entity<JobRequiredField>()
                .HasOne(jrf => jrf.Job)
                .WithMany(j => j.JobRequiredFields)
                .HasForeignKey(jrf => jrf.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobRequiredField → CandidateField
            modelBuilder.Entity<JobRequiredField>()
                .HasOne(jrf => jrf.CandidateField)
                .WithMany()
                .HasForeignKey(jrf => jrf.CandidateFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CandidateField>().HasData(
                    new CandidateField { Id = 1, FieldName = "Full Name", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 2, FieldName = "Gender", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 3, FieldName = "Date of Birth", FieldType = "Date", IsMandatory = true },
                    new CandidateField { Id = 4, FieldName = "Nationality", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 5, FieldName = "CNIC / Passport No.", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 6, FieldName = "Marital Status", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 7, FieldName = "Religion", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 8, FieldName = "Contact Number(s)", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 9, FieldName = "Email Address", FieldType = "Email", IsMandatory = true },
                    new CandidateField { Id = 10, FieldName = "Current Address", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 11, FieldName = "Permanent Address", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 12, FieldName = "LinkedIn Profile URL", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 13, FieldName = "Other Social Media / Portfolio Links", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 14, FieldName = "Professional Website / Blog", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 15, FieldName = "Photograph", FieldType = "File", IsMandatory = false },
                    new CandidateField { Id = 16, FieldName = "Job Titles Applying For", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 17, FieldName = "Job Requisition ID", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 18, FieldName = "Location(s)", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 19, FieldName = "Open to Relocation?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 20, FieldName = "Relocation Preference", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 21, FieldName = "Willing to Travel?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 22, FieldName = "Desired Employment Type", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 23, FieldName = "Remote/Hybrid/On-site Preference", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 24, FieldName = "Earliest Possible Joining Date", FieldType = "Date", IsMandatory = true },
                    new CandidateField { Id = 25, FieldName = "Notice Period", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 26, FieldName = "Highest Qualification Obtained", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 27, FieldName = "Education Level(s)", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 28, FieldName = "Institute / University Name", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 29, FieldName = "Country of Education", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 30, FieldName = "Major / Field of Study", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 31, FieldName = "Graduation Year(s)", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 32, FieldName = "GPA / Percentage / Division", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 33, FieldName = "Certifications", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 34, FieldName = "Ongoing Courses / Learning Programs", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 35, FieldName = "Employer Name", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 36, FieldName = "Industry", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 37, FieldName = "Job Title", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 38, FieldName = "Start Date – End Date", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 39, FieldName = "Employment Type", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 40, FieldName = "Job Responsibilities", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 41, FieldName = "Key Achievements", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 42, FieldName = "Reason for Leaving", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 43, FieldName = "Last Drawn Salary & Benefits", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 44, FieldName = "Reporting Manager’s Name & Contact", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 45, FieldName = "International Work Experience?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 46, FieldName = "Startup vs Corporate Experience", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 47, FieldName = "Freelance / Contract Experience", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 48, FieldName = "Technical Skills", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 49, FieldName = "Soft Skills", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 50, FieldName = "Tools/Software Knowledge", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 51, FieldName = "Programming Languages / Frameworks", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 52, FieldName = "Certifications (Professional / IT / HR / Finance / Language)", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 53, FieldName = "Language Proficiency", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 54, FieldName = "Current Industry", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 55, FieldName = "Previous Industry Experience", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 56, FieldName = "Preferred Industry", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 57, FieldName = "Sector Exposure", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 58, FieldName = "Functional Expertise", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 59, FieldName = "Sub-Domain Expertise", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 60, FieldName = "Management Level", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 61, FieldName = "Preferred Job Function", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 62, FieldName = "Preferred Work Environment", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 63, FieldName = "Preferred Industry Sector(s)", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 64, FieldName = "Preferred Currency for Salary", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 65, FieldName = "Work Authorization Status", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 66, FieldName = "Visa / Work Permit Status", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 67, FieldName = "Convicted of a crime?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 68, FieldName = "Legal restrictions for employment?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 69, FieldName = "Contractual obligations / non-compete clauses?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 70, FieldName = "Medically fit for applied role?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 71, FieldName = "Tax Registration Number", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 72, FieldName = "Willingness to undergo background check?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 73, FieldName = "Willingness to undergo medical examination?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 74, FieldName = "Reference 1: Name", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 75, FieldName = "Reference 1: CNIC #", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 76, FieldName = "Reference 1: Designation", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 77, FieldName = "Reference 1: Organization", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 78, FieldName = "Reference 1: Relationship", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 79, FieldName = "Reference 1: Contact Info", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 80, FieldName = "Reference 2: Name", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 81, FieldName = "Reference 2: CNIC #", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 82, FieldName = "Reference 2: Designation", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 83, FieldName = "Reference 2: Organization", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 84, FieldName = "Reference 2: Relationship", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 85, FieldName = "Reference 2: Contact Info", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 86, FieldName = "How did you hear about this job?", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 87, FieldName = "Referral Name (if any)", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 88, FieldName = "Availability for interviews", FieldType = "Text", IsMandatory = false },
                    new CandidateField { Id = 89, FieldName = "Upload Resume / CV", FieldType = "File", IsMandatory = true },
                    new CandidateField { Id = 90, FieldName = "Upload Cover Letter", FieldType = "File", IsMandatory = false },
                    new CandidateField { Id = 91, FieldName = "Upload Portfolio", FieldType = "File", IsMandatory = false },
                    new CandidateField { Id = 92, FieldName = "Upload Certificates / Transcripts", FieldType = "File", IsMandatory = false },
                    new CandidateField { Id = 93, FieldName = "Declaration Checkbox", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 94, FieldName = "Digital Signature / Typed Name", FieldType = "Text", IsMandatory = true },
                    new CandidateField { Id = 95, FieldName = "Date/Time of Submission", FieldType = "Date", IsMandatory = true }
                );
        }

    }
}
