using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly Dictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IDepartmentInvitationRepository DepartmentInvitations => (IDepartmentInvitationRepository)GetRepository<DepartmentInvitation>();
        public IDepartmentRepository Departments => (IDepartmentRepository)GetRepository<Department>();
        public IUserRepository Users => (IUserRepository)GetRepository<User>();
        public IRepository<Profile> Profiles => GetRepository<Profile>();
        public ITechnicalDocumentRepository TechnicalDocument => (ITechnicalDocumentRepository)GetRepository<TechnicalDocument>();
        public IRepository<RequestVersion> RequestVersions => GetRepository<RequestVersion>();
        public IAppraisalHistoryRepository AppraisalHistory => (IAppraisalHistoryRepository)GetRepository<AppraisalHistory>();
        public IAttachmentRepository Attachments => (IAttachmentRepository)GetRepository<Attachment>();
        public ITechnicalKnowledgeBaseRepository TechnicalKnowledgeBase => (ITechnicalKnowledgeBaseRepository)GetRepository<TechnicalKnowledgeBase>();
        public IRepository<Notification> Notifications => GetRepository<Notification>();
        public IUserNotificationRepository UserNotifications => (IUserNotificationRepository)GetRepository<UserNotification>();
        public IFeedbackIssueRepository FeedbackIssues => (IFeedbackIssueRepository)GetRepository<FeedbackIssue>();
        public IAppraisalAssignmentRepository AppraisalAssignments => (IAppraisalAssignmentRepository)GetRepository<AppraisalAssignment>();
        public IAppraisalReviewerRepository AppraisalReviewers => (IAppraisalReviewerRepository)GetRepository<AppraisalReviewer>();
        // public IWorkflowRepository ApprovalWorkflows => (IWorkflowRepository)GetRepository<ApprovalWorkflow>();
        public IFeedbackCommentRepository FeedbackComments => (IFeedbackCommentRepository)GetRepository<FeedbackComment>();
        public IAttachmentLinkRepository AttachmentLinks => (IAttachmentLinkRepository)GetRepository<AttachmentLink>();
        public IApprovalWorkflowRepository ApprovalWorkflows => (IApprovalWorkflowRepository)GetRepository<ApprovalWorkflow>();

        private IDashboardRepository? _dashboardRepository;

        public IDashboardRepository Dashboard => _dashboardRepository ??= _serviceProvider.GetRequiredService<IDashboardRepository>();

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (!_repositories.TryGetValue(type, out var repository))
            {
                repository = type.Name switch
                {
                    nameof(User) => _serviceProvider.GetRequiredService<IUserRepository>(),
                    nameof(DepartmentInvitation) => _serviceProvider.GetRequiredService<IDepartmentInvitationRepository>(),
                    nameof(TechnicalDocument) => _serviceProvider.GetRequiredService<ITechnicalDocumentRepository>(),
                    nameof(AppraisalHistory) => _serviceProvider.GetRequiredService<IAppraisalHistoryRepository>(),
                    nameof(UserNotification) => _serviceProvider.GetRequiredService<IUserNotificationRepository>(),
                    nameof(FeedbackIssue) => _serviceProvider.GetRequiredService<IFeedbackIssueRepository>(),
                    nameof(AppraisalAssignment) => _serviceProvider.GetRequiredService<IAppraisalAssignmentRepository>(),
                    nameof(AppraisalReviewer) => _serviceProvider.GetRequiredService<IAppraisalReviewerRepository>(),
                    nameof(Department) => _serviceProvider.GetRequiredService<IDepartmentRepository>(),
                    //nameof(ApprovalWorkflow) => _serviceProvider.GetRequiredService<IWorkflowRepository>(),
                    nameof(TechnicalKnowledgeBase) => _serviceProvider.GetRequiredService<ITechnicalKnowledgeBaseRepository>(),
                    nameof(FeedbackComment) => _serviceProvider.GetRequiredService<IFeedbackCommentRepository>(),
                    nameof(AttachmentLink) => _serviceProvider.GetRequiredService<IAttachmentLinkRepository>(),
                    nameof(Attachment) => _serviceProvider.GetRequiredService<IAttachmentRepository>(),
                    nameof(ApprovalWorkflow) => _serviceProvider.GetRequiredService<IApprovalWorkflowRepository>(),

                    _ => CreateBaseRepository<TEntity>()
                };

                _repositories.Add(type, repository!);
            }

            return (IRepository<TEntity>)repository!;
        }

        private object CreateBaseRepository<TEntity>() where TEntity : class
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<BaseRepository<TEntity>>>();
            return new BaseRepository<TEntity>(_context, logger);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null) return;
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction commit failed. Initiating rollback...");
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await ReleaseTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await ReleaseTransactionAsync();
            }
        }

        private async Task ReleaseTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _transaction?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public async Task<T> ExecuteWithStrategyAsync<T>(Func<Task<T>> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var result = await action();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Execution strategy failed during generic operation.");
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task ExecuteWithStrategyAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Execution strategy failed during operation.");
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}