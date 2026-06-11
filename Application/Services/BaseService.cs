using Application.Common;
using Application.Interfaces.Persistence;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Services
{
    public abstract class BaseService
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected BaseService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        protected async Task<(User? user, ApiResponse<T>? error)> GetUserOrErrorAsync<T>(
            Guid userId, params Expression<Func<User, object>>[] includes)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, includes);
            return user == null ? (null, ApiResponse<T>.Failure(404, "The account does not exist.")) : (user, null);
        }
    }
}
