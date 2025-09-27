using System;
using System.ComponentModel.DataAnnotations;

namespace ATSProject.Models
{
    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public required string Email { get; set; }

        [Required, MaxLength(20)]
        public required string Phone { get; set; }

        [Required, MaxLength(50)]
        public required string RoleApplied { get; set; }

        [MaxLength] // allow nvarchar(max)
        public string? Skills { get; set; }

        [MaxLength] // no length → EF maps to NVARCHAR(MAX)
        public string? Experience { get; set; }

        public string? Education { get; set; }

        public DateTime DateApplied { get; set; }

        [MaxLength(20)]
        public string? Source { get; set; }

        [MaxLength(200)]
        public string? CvFilePath { get; set; }

        [MaxLength(50)]
        public string? UniqueId { get; set; }
        public string? GmailMessageId { get; set; }
    }
}
