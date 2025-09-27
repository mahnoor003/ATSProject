using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSProject.Migrations
{
    /// <inheritdoc />
    public partial class FixJobModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobId1",
                table: "CandidateApplications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateApplications_JobId1",
                table: "CandidateApplications",
                column: "JobId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateApplications_Jobs_JobId1",
                table: "CandidateApplications",
                column: "JobId1",
                principalTable: "Jobs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateApplications_Jobs_JobId1",
                table: "CandidateApplications");

            migrationBuilder.DropIndex(
                name: "IX_CandidateApplications_JobId1",
                table: "CandidateApplications");

            migrationBuilder.DropColumn(
                name: "JobId1",
                table: "CandidateApplications");
        }
    }
}
