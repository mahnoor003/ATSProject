namespace ATSProject.Models
{
    public class CandidateField
    {
       
            public int Id { get; set; }
            public string? FieldName { get; set; } // e.g., "Full Name", "Email"
            public string? FieldType { get; set; } // e.g., "Text", "Email", "File", "Date"
            public bool IsMandatory { get; set; }

    }
}
