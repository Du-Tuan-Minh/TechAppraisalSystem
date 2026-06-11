using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttachmentLinks_FeedbackIssues_EntityId",
                table: "AttachmentLinks");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "AttachmentLinks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "FeedbackCommentId",
                table: "AttachmentLinks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeedbackIssueId",
                table: "AttachmentLinks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FeedbackComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedbackIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackComments_FeedbackComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "FeedbackComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackComments_FeedbackIssues_FeedbackIssueId",
                        column: x => x.FeedbackIssueId,
                        principalTable: "FeedbackIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentLinks_FeedbackCommentId",
                table: "AttachmentLinks",
                column: "FeedbackCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentLinks_FeedbackIssueId",
                table: "AttachmentLinks",
                column: "FeedbackIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackComments_FeedbackIssueId",
                table: "FeedbackComments",
                column: "FeedbackIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackComments_ParentCommentId",
                table: "FeedbackComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackComments_UserId",
                table: "FeedbackComments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentLinks_FeedbackComments_FeedbackCommentId",
                table: "AttachmentLinks",
                column: "FeedbackCommentId",
                principalTable: "FeedbackComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentLinks_FeedbackIssues_FeedbackIssueId",
                table: "AttachmentLinks",
                column: "FeedbackIssueId",
                principalTable: "FeedbackIssues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttachmentLinks_FeedbackComments_FeedbackCommentId",
                table: "AttachmentLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_AttachmentLinks_FeedbackIssues_FeedbackIssueId",
                table: "AttachmentLinks");

            migrationBuilder.DropTable(
                name: "FeedbackComments");

            migrationBuilder.DropIndex(
                name: "IX_AttachmentLinks_FeedbackCommentId",
                table: "AttachmentLinks");

            migrationBuilder.DropIndex(
                name: "IX_AttachmentLinks_FeedbackIssueId",
                table: "AttachmentLinks");

            migrationBuilder.DropColumn(
                name: "FeedbackCommentId",
                table: "AttachmentLinks");

            migrationBuilder.DropColumn(
                name: "FeedbackIssueId",
                table: "AttachmentLinks");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "AttachmentLinks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentLinks_FeedbackIssues_EntityId",
                table: "AttachmentLinks",
                column: "EntityId",
                principalTable: "FeedbackIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
