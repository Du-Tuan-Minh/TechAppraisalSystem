using Application.Hubs;
using Application.Interfaces.Authentication;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace Application.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthService> _authService;
        private readonly Lazy<IUserService> _userService;
        private readonly Lazy<IDepartmentService> _departmentService;
        private readonly Lazy<IAppraisalService> _appraisalService;
        private readonly Lazy<ISigningService> _signingService;
        private readonly Lazy<IFeedbackService> _feedbackService;
        private readonly Lazy<INotificationService> _notificationService;
        private readonly Lazy<ITechnicalDocumentService> _technicalDocumentService;
        private readonly Lazy<IAiService> _aiService;
        private readonly Lazy<IAppraisalHistoryService> _appraisalHistoryService;
        private readonly Lazy<IAttachmentService> _attachmentService;
        private readonly Lazy<IDashboardService> _dashboardService;
        //private readonly Lazy<IWorkflowService> _workflowService;
        private readonly Lazy<ITechnicalKnowledgeBaseService> _technicalKnowledgeBaseService;
        private readonly Lazy<IFeedbackCommentService> _feedbackCommentService;
        private readonly Lazy<IApprovalWorkflowService> _approvalWorkflowService;

        public ServiceManager(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _authService = new Lazy<IAuthService>(() => new AuthService(unitOfWork, tokenService));
            _userService = new Lazy<IUserService>(() => new UserService(unitOfWork, mapper));
            _departmentService = new Lazy<IDepartmentService>(() => new DepartmentService(unitOfWork, mapper));
            _technicalDocumentService = new Lazy<ITechnicalDocumentService>(() => new TechnicalDocumentService(unitOfWork, mapper, NotificationService));
            _appraisalService = new Lazy<IAppraisalService>(() => new AppraisalService(unitOfWork, mapper, NotificationService));
            _signingService = new Lazy<ISigningService>(() => new SigningService(unitOfWork, mapper, NotificationService));
            _feedbackService = new Lazy<IFeedbackService>(() => new FeedbackService(unitOfWork, mapper));
            _notificationService = new Lazy<INotificationService>(() => new NotificationService(unitOfWork, mapper, hubContext));
            // _aiService = new Lazy<IAiService>(() => new AiService(unitOfWork, mapper));
            _appraisalHistoryService = new Lazy<IAppraisalHistoryService>(() => new AppraisalHistoryService(unitOfWork, mapper, NotificationService));
            _attachmentService = new Lazy<IAttachmentService>(() => new AttachmentService(unitOfWork, mapper));
            _dashboardService = new Lazy<IDashboardService>(() => new DashboardService(unitOfWork, mapper));
            // _workflowService = new Lazy<IWorkflowService>(() => new WorkflowService(unitOfWork, mapper));
            _technicalKnowledgeBaseService = new Lazy<ITechnicalKnowledgeBaseService>(() => new TechnicalKnowledgeBaseService(unitOfWork, mapper));
            _feedbackCommentService = new Lazy<IFeedbackCommentService>(() => new FeedbackCommentService(unitOfWork, mapper, hubContext));
            _approvalWorkflowService = new Lazy<IApprovalWorkflowService>(() => new ApprovalWorkflowService(unitOfWork, mapper));
        }

        public IAuthService AuthService => _authService.Value;
        public ITechnicalDocumentService TechnicalDocumentService => _technicalDocumentService.Value;
        public IDepartmentService DepartmentService => _departmentService.Value;
        public IAppraisalService AppraisalService => _appraisalService.Value;
        public ISigningService SigningService => _signingService.Value;
        public IFeedbackService FeedbackService => _feedbackService.Value;
        public INotificationService NotificationService => _notificationService.Value;
        public IUserService UserService => _userService.Value;
        public IAiService AiService => _aiService.Value;
        public IAppraisalHistoryService AppraisalHistoryService => _appraisalHistoryService.Value;
        public IAttachmentService AttachmentService => _attachmentService.Value;
        public IDashboardService DashboardService => _dashboardService.Value;
        //public IWorkflowService WorkflowService => _workflowService.Value;
        public ITechnicalKnowledgeBaseService TechnicalKnowledgeBaseService => _technicalKnowledgeBaseService.Value;
        public IFeedbackCommentService FeedbackCommentService => _feedbackCommentService.Value;
        public IApprovalWorkflowService ApprovalWorkflowService => _approvalWorkflowService.Value;
    }
}