namespace ATSProject.Models
{
    public class JobRequiredField
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int CandidateFieldId { get; set; }

        public virtual Job? Job { get; set; }
        public virtual CandidateField? CandidateField { get; set; }
    }
}
