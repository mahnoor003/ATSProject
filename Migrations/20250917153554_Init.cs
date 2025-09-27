using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATSProject.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RoleApplied = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Experience = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateApplied = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CvFilePath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UniqueId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GmailMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResumePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateApplications_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobRequiredFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    CandidateFieldId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRequiredFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobRequiredFields_CandidateFields_CandidateFieldId",
                        column: x => x.CandidateFieldId,
                        principalTable: "CandidateFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobRequiredFields_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidateResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    CandidateFieldId = table.Column<int>(type: "int", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CandidateApplicationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateResponses_CandidateApplications_CandidateApplicationId",
                        column: x => x.CandidateApplicationId,
                        principalTable: "CandidateApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateResponses_CandidateFields_CandidateFieldId",
                        column: x => x.CandidateFieldId,
                        principalTable: "CandidateFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateResponses_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CandidateFields",
                columns: new[] { "Id", "FieldName", "FieldType", "IsMandatory" },
                values: new object[,]
                {
                    { 1, "Full Name", "Text", true },
                    { 2, "Gender", "Text", true },
                    { 3, "Date of Birth", "Date", true },
                    { 4, "Nationality", "Text", true },
                    { 5, "CNIC / Passport No.", "Text", true },
                    { 6, "Marital Status", "Text", false },
                    { 7, "Religion", "Text", false },
                    { 8, "Contact Number(s)", "Text", true },
                    { 9, "Email Address", "Email", true },
                    { 10, "Current Address", "Text", true },
                    { 11, "Permanent Address", "Text", true },
                    { 12, "LinkedIn Profile URL", "Text", false },
                    { 13, "Other Social Media / Portfolio Links", "Text", false },
                    { 14, "Professional Website / Blog", "Text", false },
                    { 15, "Photograph", "File", false },
                    { 16, "Job Titles Applying For", "Text", true },
                    { 17, "Job Requisition ID", "Text", false },
                    { 18, "Location(s)", "Text", true },
                    { 19, "Open to Relocation?", "Text", false },
                    { 20, "Relocation Preference", "Text", false },
                    { 21, "Willing to Travel?", "Text", false },
                    { 22, "Desired Employment Type", "Text", true },
                    { 23, "Remote/Hybrid/On-site Preference", "Text", false },
                    { 24, "Earliest Possible Joining Date", "Date", true },
                    { 25, "Notice Period", "Text", false },
                    { 26, "Highest Qualification Obtained", "Text", true },
                    { 27, "Education Level(s)", "Text", false },
                    { 28, "Institute / University Name", "Text", true },
                    { 29, "Country of Education", "Text", false },
                    { 30, "Major / Field of Study", "Text", true },
                    { 31, "Graduation Year(s)", "Text", false },
                    { 32, "GPA / Percentage / Division", "Text", false },
                    { 33, "Certifications", "Text", false },
                    { 34, "Ongoing Courses / Learning Programs", "Text", false },
                    { 35, "Employer Name", "Text", true },
                    { 36, "Industry", "Text", true },
                    { 37, "Job Title", "Text", true },
                    { 38, "Start Date – End Date", "Text", true },
                    { 39, "Employment Type", "Text", true },
                    { 40, "Job Responsibilities", "Text", false },
                    { 41, "Key Achievements", "Text", false },
                    { 42, "Reason for Leaving", "Text", false },
                    { 43, "Last Drawn Salary & Benefits", "Text", false },
                    { 44, "Reporting Manager’s Name & Contact", "Text", false },
                    { 45, "International Work Experience?", "Text", false },
                    { 46, "Startup vs Corporate Experience", "Text", false },
                    { 47, "Freelance / Contract Experience", "Text", false },
                    { 48, "Technical Skills", "Text", true },
                    { 49, "Soft Skills", "Text", false },
                    { 50, "Tools/Software Knowledge", "Text", false },
                    { 51, "Programming Languages / Frameworks", "Text", false },
                    { 52, "Certifications (Professional / IT / HR / Finance / Language)", "Text", false },
                    { 53, "Language Proficiency", "Text", false },
                    { 54, "Current Industry", "Text", true },
                    { 55, "Previous Industry Experience", "Text", false },
                    { 56, "Preferred Industry", "Text", false },
                    { 57, "Sector Exposure", "Text", false },
                    { 58, "Functional Expertise", "Text", false },
                    { 59, "Sub-Domain Expertise", "Text", false },
                    { 60, "Management Level", "Text", true },
                    { 61, "Preferred Job Function", "Text", false },
                    { 62, "Preferred Work Environment", "Text", false },
                    { 63, "Preferred Industry Sector(s)", "Text", false },
                    { 64, "Preferred Currency for Salary", "Text", false },
                    { 65, "Work Authorization Status", "Text", false },
                    { 66, "Visa / Work Permit Status", "Text", false },
                    { 67, "Convicted of a crime?", "Text", false },
                    { 68, "Legal restrictions for employment?", "Text", false },
                    { 69, "Contractual obligations / non-compete clauses?", "Text", false },
                    { 70, "Medically fit for applied role?", "Text", false },
                    { 71, "Tax Registration Number", "Text", false },
                    { 72, "Willingness to undergo background check?", "Text", false },
                    { 73, "Willingness to undergo medical examination?", "Text", false },
                    { 74, "Reference 1: Name", "Text", false },
                    { 75, "Reference 1: CNIC #", "Text", false },
                    { 76, "Reference 1: Designation", "Text", false },
                    { 77, "Reference 1: Organization", "Text", false },
                    { 78, "Reference 1: Relationship", "Text", false },
                    { 79, "Reference 1: Contact Info", "Text", false },
                    { 80, "Reference 2: Name", "Text", false },
                    { 81, "Reference 2: CNIC #", "Text", false },
                    { 82, "Reference 2: Designation", "Text", false },
                    { 83, "Reference 2: Organization", "Text", false },
                    { 84, "Reference 2: Relationship", "Text", false },
                    { 85, "Reference 2: Contact Info", "Text", false },
                    { 86, "How did you hear about this job?", "Text", false },
                    { 87, "Referral Name (if any)", "Text", false },
                    { 88, "Availability for interviews", "Text", false },
                    { 89, "Upload Resume / CV", "File", true },
                    { 90, "Upload Cover Letter", "File", false },
                    { 91, "Upload Portfolio", "File", false },
                    { 92, "Upload Certificates / Transcripts", "File", false },
                    { 93, "Declaration Checkbox", "Text", true },
                    { 94, "Digital Signature / Typed Name", "Text", true },
                    { 95, "Date/Time of Submission", "Date", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_JobId",
                table: "CandidateApplications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateResponses_CandidateApplicationId",
                table: "CandidateResponses",
                column: "CandidateApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateResponses_CandidateFieldId",
                table: "CandidateResponses",
                column: "CandidateFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateResponses_JobId",
                table: "CandidateResponses",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRequiredFields_CandidateFieldId",
                table: "JobRequiredFields",
                column: "CandidateFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRequiredFields_JobId",
                table: "JobRequiredFields",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateResponses");

            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "JobRequiredFields");

            migrationBuilder.DropTable(
                name: "CandidateApplications");

            migrationBuilder.DropTable(
                name: "CandidateFields");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
