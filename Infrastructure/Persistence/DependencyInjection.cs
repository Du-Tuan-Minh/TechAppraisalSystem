using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
   
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, o => o.UseVector()));

            return services;
        }
    }
}