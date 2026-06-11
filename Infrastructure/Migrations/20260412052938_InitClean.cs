using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameDepartment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CodeDepartment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteeEmployeeCode = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InvitationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentInvitations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HashPassword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DocumentCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    QrCode = table.Column<string>(type: "text", nullable: true),
                    CurrentHandlerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalDocuments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TechnicalDocuments_Users_CurrentHandlerId",
                        column: x => x.CurrentHandlerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TechnicalDocuments_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalKnowledgeBase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LinkedSpecPattern = table.Column<string>(type: "jsonb", nullable: false),
                    TechnicalSolution = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    OccurrenceCount = table.Column<int>(type: "integer", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    LastVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalKnowledgeBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalKnowledgeBase_Users_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppraisalAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsibleManagerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ManagerComment = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppraisalAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppraisalAssignments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppraisalAssignments_TechnicalDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "TechnicalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppraisalAssignments_Users_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppraisalAssignments_Users_ResponsibleManagerId",
                        column: x => x.ResponsibleManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppraisalAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppraisalReviewers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TaskDescription = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppraisalReviewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppraisalReviewers_AppraisalAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "AppraisalAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppraisalReviewers_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppraisalHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandlerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldStatus = table.Column<string>(type: "text", nullable: false),
                    NewStatus = table.Column<string>(type: "text", nullable: false),
                    AppraisalAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppraisalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppraisalHistories_AppraisalAssignments_AppraisalAssignment~",
                        column: x => x.AppraisalAssignmentId,
                        principalTable: "AppraisalAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppraisalHistories_TechnicalDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "TechnicalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppraisalHistories_Users_HandlerId",
                        column: x => x.HandlerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppraisalAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    RequiredRole = table.Column<string>(type: "text", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCurrentStep = table.Column<bool>(type: "boolean", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkflows_AppraisalAssignments_AppraisalAssignmentId",
                        column: x => x.AppraisalAssignmentId,
                        principalTable: "AppraisalAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkflows_TechnicalDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "TechnicalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkflows_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileData = table.Column<byte[]>(type: "bytea", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentCategory = table.Column<string>(type: "text", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_TechnicalDocuments_TechnicalDocumentId",
                        column: x => x.TechnicalDocumentId,
                        principalTable: "TechnicalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachments_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorPath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IssueCategory = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AppraisalHistoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    TechnicalKnowledgeBaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedInVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppraisalAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackIssues_AppraisalAssignments_AppraisalAssignmentId",
                        column: x => x.AppraisalAssignmentId,
                        principalTable: "AppraisalAssignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeedbackIssues_AppraisalHistories_AppraisalHistoryId",
                        column: x => x.AppraisalHistoryId,
                        principalTable: "AppraisalHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FeedbackIssues_Departments_AssignedDepartmentId",
                        column: x => x.AssignedDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackIssues_TechnicalDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "TechnicalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackIssues_TechnicalKnowledgeBase_TechnicalKnowledgeBas~",
                        column: x => x.TechnicalKnowledgeBaseId,
                        principalTable: "TechnicalKnowledgeBase",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeedbackIssues_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    TechnicalSpecsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceIssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestVersions_FeedbackIssues_SourceIssueId",
                        column: x => x.SourceIssueId,
                        principalTable: "FeedbackIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RequestVersions_TechnicalDocuments_RequestId",
                        column: x => x.RequestId,
                        principalTable: "TechnicalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalAssignments_AssignedById",
                table: "AppraisalAssignments",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalAssignments_DepartmentId",
                table: "AppraisalAssignments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalAssignments_DocumentId",
                table: "AppraisalAssignments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalAssignments_DocumentId_RequestVersionId",
                table: "AppraisalAssignments",
                columns: new[] { "DocumentId", "RequestVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalAssignments_RequestVersionId",
                table: "AppraisalAssignments",
                column: "RequestVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalAssignments_ResponsibleManagerId",
                table: "AppraisalAssignments",
                column: "ResponsibleManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalAssignments_UserId",
                table: "AppraisalAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalHistories_AppraisalAssignmentId",
                table: "AppraisalHistories",
                column: "AppraisalAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalHistories_DocumentId",
                table: "AppraisalHistories",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalHistories_HandlerId",
                table: "AppraisalHistories",
                column: "HandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalHistories_RequestVersionId",
                table: "AppraisalHistories",
                column: "RequestVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalReviewers_AssignmentId",
                table: "AppraisalReviewers",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppraisalReviewers_StaffId",
                table: "AppraisalReviewers",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_AppraisalAssignmentId",
                table: "ApprovalWorkflows",
                column: "AppraisalAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_ApproverId",
                table: "ApprovalWorkflows",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_DocumentId_StepOrder",
                table: "ApprovalWorkflows",
                columns: new[] { "DocumentId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_IsCurrentStep",
                table: "ApprovalWorkflows",
                column: "IsCurrentStep");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_RequestVersionId",
                table: "ApprovalWorkflows",
                column: "RequestVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentLinks_AttachmentId",
                table: "AttachmentLinks",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentLinks_EntityId_EntityType",
                table: "AttachmentLinks",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentLinks_EntityId_EntityType_AttachmentId",
                table: "AttachmentLinks",
                columns: new[] { "EntityId", "EntityType", "AttachmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_RequestVersionId",
                table: "Attachments",
                column: "RequestVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TechnicalDocumentId",
                table: "Attachments",
                column: "TechnicalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UploadedById",
                table: "Attachments",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentInvitations_DepartmentId",
                table: "DepartmentInvitations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentInvitations_InvitationCode",
                table: "DepartmentInvitations",
                column: "InvitationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentInvitations_InviteeEmployeeCode",
                table: "DepartmentInvitations",
                column: "InviteeEmployeeCode");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CodeDepartment",
                table: "Departments",
                column: "CodeDepartment",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentId",
                table: "Departments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_AppraisalAssignmentId",
                table: "FeedbackIssues",
                column: "AppraisalAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_AppraisalHistoryId",
                table: "FeedbackIssues",
                column: "AppraisalHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_AssignedDepartmentId",
                table: "FeedbackIssues",
                column: "AssignedDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_DocumentId",
                table: "FeedbackIssues",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_DocumentId_Status",
                table: "FeedbackIssues",
                columns: new[] { "DocumentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_ReporterId",
                table: "FeedbackIssues",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_RequestVersionId",
                table: "FeedbackIssues",
                column: "RequestVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_ResolvedInVersionId",
                table: "FeedbackIssues",
                column: "ResolvedInVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_Status",
                table: "FeedbackIssues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackIssues_TechnicalKnowledgeBaseId",
                table: "FeedbackIssues",
                column: "TechnicalKnowledgeBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestVersions_RequestId_VersionNumber",
                table: "RequestVersions",
                columns: new[] { "RequestId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestVersions_SourceIssueId",
                table: "RequestVersions",
                column: "SourceIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestVersions_TechnicalSpecsJson",
                table: "RequestVersions",
                column: "TechnicalSpecsJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDocuments_CurrentHandlerId",
                table: "TechnicalDocuments",
                column: "CurrentHandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDocuments_DepartmentId",
                table: "TechnicalDocuments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDocuments_DocumentCode",
                table: "TechnicalDocuments",
                column: "DocumentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDocuments_RequesterId",
                table: "TechnicalDocuments",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalDocuments_Status",
                table: "TechnicalDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalKnowledgeBase_LinkedSpecPattern",
                table: "TechnicalKnowledgeBase",
                column: "LinkedSpecPattern")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalKnowledgeBase_Title",
                table: "TechnicalKnowledgeBase",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalKnowledgeBase_VerifiedById",
                table: "TechnicalKnowledgeBase",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_NotificationId",
                table: "UserNotifications",
                columns: new[] { "UserId", "NotificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeCode",
                table: "Users",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalAssignments_RequestVersions_RequestVersionId",
                table: "AppraisalAssignments",
                column: "RequestVersionId",
                principalTable: "RequestVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppraisalHistories_RequestVersions_RequestVersionId",
                table: "AppraisalHistories",
                column: "RequestVersionId",
                principalTable: "RequestVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalWorkflows_RequestVersions_RequestVersionId",
                table: "ApprovalWorkflows",
                column: "RequestVersionId",
                principalTable: "RequestVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentLinks_Attachments_AttachmentId",
                table: "AttachmentLinks",
                column: "AttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttachmentLinks_FeedbackIssues_EntityId",
                table: "AttachmentLinks",
                column: "EntityId",
                principalTable: "FeedbackIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_RequestVersions_RequestVersionId",
                table: "Attachments",
                column: "RequestVersionId",
                principalTable: "RequestVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackIssues_RequestVersions_RequestVersionId",
                table: "FeedbackIssues",
                column: "RequestVersionId",
                principalTable: "RequestVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackIssues_RequestVersions_ResolvedInVersionId",
                table: "FeedbackIssues",
                column: "ResolvedInVersionId",
                principalTable: "RequestVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalAssignments_Departments_DepartmentId",
                table: "AppraisalAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackIssues_Departments_AssignedDepartmentId",
                table: "FeedbackIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_TechnicalDocuments_Departments_DepartmentId",
                table: "TechnicalDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalAssignments_RequestVersions_RequestVersionId",
                table: "AppraisalAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AppraisalHistories_RequestVersions_RequestVersionId",
                table: "AppraisalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackIssues_RequestVersions_RequestVersionId",
                table: "FeedbackIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackIssues_RequestVersions_ResolvedInVersionId",
                table: "FeedbackIssues");

            migrationBuilder.DropTable(
                name: "AppraisalReviewers");

            migrationBuilder.DropTable(
                name: "ApprovalWorkflows");

            migrationBuilder.DropTable(
                name: "AttachmentLinks");

            migrationBuilder.DropTable(
                name: "DepartmentInvitations");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "RequestVersions");

            migrationBuilder.DropTable(
                name: "FeedbackIssues");

            migrationBuilder.DropTable(
                name: "AppraisalHistories");

            migrationBuilder.DropTable(
                name: "TechnicalKnowledgeBase");

            migrationBuilder.DropTable(
                name: "AppraisalAssignments");

            migrationBuilder.DropTable(
                name: "TechnicalDocuments");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
