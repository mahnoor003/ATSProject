namespace ATSProject.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string? JobTitle { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ A Job can have many Applications
        public virtual ICollection<CandidateApplication> CandidateApplications { get; set; }
            = new List<CandidateApplication>();

        // already present
        public ICollection<CandidateResponse> CandidateResponses { get; set; } = new List<CandidateResponse>();
        public virtual ICollection<JobRequiredField>? JobRequiredFields { get; set; }
    }
}
