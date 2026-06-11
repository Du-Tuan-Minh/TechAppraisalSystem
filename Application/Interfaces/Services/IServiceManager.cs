namespace Application.Interfaces.Services
{
    public interface IServiceManager
    {
        IAuthService AuthService { get; }
        IUserService UserService { get; }
        IDepartmentService DepartmentService { get; }
        ITechnicalDocumentService TechnicalDocumentService { get; }
        IAppraisalService AppraisalService { get; }
        ISigningService SigningService { get; }
        IFeedbackService FeedbackService { get; }
        IAiService AiService { get; }
        INotificationService NotificationService { get; }
        IAppraisalHistoryService AppraisalHistoryService { get; }
        IAttachmentService AttachmentService { get; }
        IDashboardService DashboardService { get; }
        // IWorkflowService WorkflowService { get; }
        ITechnicalKnowledgeBaseService TechnicalKnowledgeBaseService { get; }
        IFeedbackCommentService FeedbackCommentService { get; }
        IApprovalWorkflowService ApprovalWorkflowService { get; }
    }
}