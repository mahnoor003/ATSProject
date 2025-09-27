using System.Collections.Generic;

namespace ATSProject.Models
{
    public class CandidateApplication
    {
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job? Job { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ResumePath { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // 👇 Add this navigation property
        public ICollection<CandidateResponse> CandidateResponses { get; set; } = new List<CandidateResponse>();
    }
}
