using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasktower.ProjectService.DataAccess.Context;
using Tasktower.ProjectService.DataAccess.Repositories;

namespace Tasktower.ProjectService.Configuration.StartupExtensions
{
    public static class DataAccessServicesConfig
    {
        public static void ConfigureDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BoardDBContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SQLServer"));
            });
        }

        public static void ConfigureRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectRoleRepository, ProjectRoleRepository>();
            services.AddScoped<ITaskBoardRepository, TaskBoardRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
        
        public static void UpdateDatabase(this IApplicationBuilder app, IConfiguration configuration)
        {
        if (!configuration.GetValue("Migration:Migrate", false)) return;
        using var serviceScope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();
        using var context = serviceScope.ServiceProvider.GetService<BoardDBContext>();
        context?.Database.Migrate();
        // Todo: setup test data
        }
    }
}