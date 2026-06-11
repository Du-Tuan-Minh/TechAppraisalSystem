namespace Application.DTOs
{
    public class DashboardSummaryStaffDto
    {
        public int DraftCount { get; set; }
        public int ReturnedForRevisionCount { get; set; }
        public int InternalReviewCount { get; set; }
        public int AppraisalCount { get; set; }
        public int SigningCount { get; set; }
        public int IssuedCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class DashboardSummaryManagerDto
    {
        public int ReviewingDocuments { get; set; }
        public int NeedConfirmationCount { get; set; }
        public int RejectedCount { get; set; }
        public int OverdueReviewDocuments { get; set; }
    }

    public class DepartmentDocumentStatusSummaryDto
    {
        public int ReviewingDocuments { get; set; }
        public int OverdueReviewDocuments { get; set; }
        public int IssuedDocuments { get; set; }
        public int RejectedDocuments { get; set; }
    }

    public class DashboardSummaryDirectorDto
    {
        public int DraftingCount { get; set; }
        public int AppraisingCount { get; set; }
        public int SigningCount { get; set; }
        public int IssuedCount { get; set; }
        public int RejectedCount { get; set; }
        public int OverdueSigningCount { get; set; }
    }

    public class ManagerWorkloadDto
    {
        public Guid ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int PendingAssignments { get; set; }
        public int InReviewAssignments { get; set; }
        public int OverdueAssignments { get; set; }
    }

    public class DashboardSummaryCoordinatorDto
    {
        public int TotalDepartments { get; set; }
        public int PendingAssignments { get; set; }
    }
}