using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalProposerIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalHistories_TechnicalDocuments_DocumentId",
                table: "AppraisalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalHistories_Users_HandlerId",
                table: "AppraisalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackIssues_Users_ReporterId",
                table: "FeedbackIssues");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalProposerIds",
                table: "TechnicalDocuments",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalHistories_TechnicalDocuments_DocumentId",
                table: "AppraisalHistories",
                column: "DocumentId",
                principalTable: "TechnicalDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalHistories_Users_HandlerId",
                table: "AppraisalHistories",
                column: "HandlerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackIssues_Users_ReporterId",
                table: "FeedbackIssues",
                column: "ReporterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalHistories_TechnicalDocuments_DocumentId",
                table: "AppraisalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalHistories_Users_HandlerId",
                table: "AppraisalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackIssues_Users_ReporterId",
                table: "FeedbackIssues");

            migrationBuilder.DropColumn(
                name: "ApprovalProposerIds",
                table: "TechnicalDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalHistories_TechnicalDocuments_DocumentId",
                table: "AppraisalHistories",
                column: "DocumentId",
                principalTable: "TechnicalDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalHistories_Users_HandlerId",
                table: "AppraisalHistories",
                column: "HandlerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackIssues_Users_ReporterId",
                table: "FeedbackIssues",
                column: "ReporterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
