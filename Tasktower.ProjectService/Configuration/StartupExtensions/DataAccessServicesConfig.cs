using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasktower.ProjectService.DataAccess.Context;
using Tasktower.ProjectService.DataAccess.Entities;
using Tasktower.ProjectService.DataAccess.Repositories;
using Tasktower.ProjectService.Tools.Constants;

namespace Tasktower.ProjectService.Configuration.StartupExtensions
{
    public static class DataAccessServicesConfig
    {
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
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

            // Attempt migration, if fail due to network connection retry again
            foreach (var _ in Enumerable.Range(1, 50))
            {
                try
                {
                    context?.Database.MigrateAsync().Wait();
                    break;
                }
                catch (Exception e)
                {
                    Task.Delay(1000);
                    Console.WriteLine(e);
                }
            }
            // Test data setup
            if (configuration.GetValue("Migration:SetupTestData", false))
            {
                var unitOfWork = serviceScope.ServiceProvider.GetService<IUnitOfWork>();
                
                SetupTestData(unitOfWork).Wait();
            }
            // Shutdown app if specified
            if (configuration.GetValue("Migration:Shutdown", false))
            {
                var appLifetime = serviceScope.ServiceProvider.GetService<IHostApplicationLifetime>();
                appLifetime?.StopApplication();
            }
        }
        
        // ---------------------------------------- Test data startup ---------------------------------
        
        private static async Task SetupTestData(IUnitOfWork unitOfWork) {
            // ___Clear data___
            await unitOfWork.TaskRepository.DeleteAll();
            await unitOfWork.TaskBoardRepository.DeleteAll();
            await unitOfWork.ProjectRoleRepository.DeleteAll();
            await unitOfWork.ProjectRepository.DeleteAll();
            await unitOfWork.UserRepository.DeleteAll();
            
            // ___Add Data___
            
            // User 1
            var user1 = new UserEntity()
            {
                UserId = "auth0|60d43ad7d0b60f006878326f",
                UserName = "adminuser",
                Picture = "https://s.gravatar.com/avatar/64e1b8d34f425d19e1ee2ea7236d3028?s=480&r=pg&d=https%3A%2F%2Fcdn.auth0.com%2Favatars%2Fad.png"
            };
            user1.UpdateAuditProperties("SYSTEM", true);
            await unitOfWork.UserRepository.Insert(user1);

            // Project 1
            var project1 = new ProjectEntity()
            {
                Title = "Admin Project",
                Description = "Test project for admin",
                ProjectRoles = new List<ProjectRoleEntity>()
                {
                    new()
                    {
                        UserEntity = user1,
                        Role = ProjectRoleValue.OWNER,
                        PendingInvite = false,
                    }
                },
                TaskBoards = new List<TaskBoardEntity>()
                {
                    new ()
                    {
                        Title = "My Board",
                        Description = "A simple task board",
                        Columns = new List<string>() {"To Do", "Doing", "Done"},
                        Tasks = new List<TaskEntity>()
                        {
                            new ()
                            {
                                Name = "Turn off the stove",
                                Summary = "Turn off the stove please",
                                TaskDescriptionMarkup = "_Turn off the stove_",
                                Column = "To Do"
                            },
                            new ()
                            {
                                Name = "Homework",
                                Summary = "Do your homework",
                                TaskDescriptionMarkup = "# Do your homework",
                                Column = "Doing"
                            },
                            new ()
                            {
                                Name = "Groceries",
                                Summary = "Buy food",
                                TaskDescriptionMarkup = "# Get food",
                                Column = "Done"
                            },
                        }
                    }
                }
            };
            project1.UpdateAuditProperties(user1.Id, true);
            foreach (var role in project1.ProjectRoles)
            {
                role.UpdateAuditProperties(user1.Id, true);
            }

            foreach (var board in project1.TaskBoards)
            {
                board.UpdateAuditProperties(user1.Id, true);
                foreach (var task in board.Tasks)
                {
                    task.UpdateAuditProperties(user1.Id, true);
                }
            }
            await unitOfWork.ProjectRepository.Insert(project1);
            
            // Save changes
            await unitOfWork.SaveChanges();
        }
    }
}