using ATSProject.Models;

public class CandidateResponse
{
    public int Id { get; set; }

    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    public int CandidateFieldId { get; set; }
    public CandidateField CandidateField { get; set; } = null!;

    public string? Response { get; set; }   // ✅ keep Response instead of Value

    // 👇 Add this
    public int CandidateApplicationId { get; set; }
    public CandidateApplication Application { get; set; } = null!;
}
