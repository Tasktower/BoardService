using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Tasktower.Lib.Aspnetcore.Errors;
using Tasktower.Lib.Aspnetcore.Paging;
using Tasktower.Lib.Aspnetcore.Security;
using Tasktower.Lib.Aspnetcore.Services.Contexts;
using Tasktower.ProjectService.BusinessLogic;
using Tasktower.ProjectService.DataAccess.Entities;
using Tasktower.ProjectService.DataAccess.Repositories;
using Tasktower.ProjectService.Dtos;
using Tasktower.ProjectService.Errors;
using Tasktower.ProjectService.Tests.TestTools.Helpers;
using Tasktower.ProjectService.Tools.Constants;
using Xunit;

namespace Tasktower.ProjectService.Tests.BusinessLogic
{
    public sealed class ProjectServiceTest : IDisposable
    {
        // Owner 1 Data
        private const string User1Id = "10001";
        private const string User1UserName = "JohnStewart";
        private readonly HashSet<string> _user1Permissions = new() {SecurityGroups.TasktowerAdmin};
        
        private readonly Guid _project1AId = Guid.Parse("0eca324e-a333-4e68-9b0a-ebeef00b60ec");
        private const string Project1ATitle = "Project 1A";
        private const string Project1ADescription = "Project 1A description";
        
        private readonly Guid _project1BId = Guid.Parse("9cd8a7ba-a6c8-44da-b310-d9a8afca525f");
        private const string Project1BTitle = "Project 1B";
        private const string Project1BDescription = "Project 1B description";
        
        // Owner 2 Data
        private const string User2Id = "10002";
        private const string User2UserName = "SteveRodgers";
        private readonly HashSet<string> _user2Permissions = new() {SecurityGroups.TasktowerAdmin};
        
        private readonly Guid _project2AId = Guid.Parse("2fceb781-c31c-4f47-9689-3916ad0bcbe8");
        private const string Project2ATitle = "Project 2A";
        private const string Project2ADescription = "Project 2A description";
        
        // Owner 3 Data
        private const string User3Id = "10003";
        private const string User3UserName = "GorillaGrodd";
        private readonly HashSet<string> _user3Permissions = new();
        
        private readonly Guid _project3AId = Guid.Parse("e52a6e70-1ffd-4439-8f8a-ba8dbfea9efe");
        private const string Project3ATitle = "Project 3A";
        private const string Project3ADescription = "Project 3A description";
        
        private readonly Guid _project3BId = Guid.Parse("9c831b5f-6956-4954-9bc3-7c325bbfe0c2");
        private const string Project3BTitle = "Project 3B";
        private const string Project3BDescription = "Project 3B description";

        private const int ProjectsTotal = 5;

        private readonly IProjectsService _projectsService;
        private readonly IUserContextAccessorService _userContextAccessorService;
        private readonly IUnitOfWork _unitOfWork;
        
        public ProjectServiceTest(IProjectsService projectsService, IUserContextAccessorService userContextAccessorService, IUnitOfWork unitOfWork)
        {
            _projectsService = projectsService;
            _userContextAccessorService = userContextAccessorService;
            _unitOfWork = unitOfWork;
            InitData().Wait();
        }
        
        public void Dispose()
        {
            _userContextAccessorService.SignOutForTesting();
            _unitOfWork.UserRepository.DeleteAll().Wait();
            _unitOfWork.ProjectRepository.DeleteAll().Wait();
            _unitOfWork.ProjectRoleRepository.DeleteAll().Wait();
            _unitOfWork.SaveChanges().Wait();
        }

        private async Task InitData()
        {
            // Owner 1 projects
            var user1Entity = new UserEntity()
            {
                UserId = User1Id,
                UserName = User1UserName,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test",
                ModifiedAt = DateTime.Now,
                ModifiedBy = "Test"
            };
            var user1Projects = new[]
            {
                new ProjectEntity
                {
                    Id = _project1AId,
                    Title = Project1ATitle,
                    Description = Project1ADescription,
                    ProjectRoles = new [] {NewProjectRole(user1Entity)},
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test",
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = "Test"
                },
                new ProjectEntity
                {
                    Id = _project1BId,
                    Title = Project1BTitle,
                    Description = Project1BDescription,
                    ProjectRoles = new [] {NewProjectRole(user1Entity)},
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test",
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = "Test"
                }
            };
            await _unitOfWork.UserRepository.Insert(user1Entity);
            await _unitOfWork.ProjectRepository.InsertMany(user1Projects);
            // Owner 2 projects
            var user2Entity = new UserEntity()
            {
                UserId = User2Id,
                UserName = User2UserName,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test",
                ModifiedAt = DateTime.Now,
                ModifiedBy = "Test"
            };
            var user2Projects = new[]
            {
                new ProjectEntity
                {
                    Id = _project2AId,
                    Title = Project2ATitle,
                    Description = Project2ADescription,
                    ProjectRoles = new[] {NewProjectRole(user2Entity)},
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test",
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = "Test"
                }
            };
            await _unitOfWork.ProjectRepository.InsertMany(user2Projects);
            // Owner 3 projects
            var user3Entity = new UserEntity()
            {
                UserId = User3Id,
                UserName = User3UserName,
                CreatedAt = DateTime.Now,
                CreatedBy = "Test",
                ModifiedAt = DateTime.Now,
                ModifiedBy = "Test"
            };
            await _unitOfWork.ProjectRepository.InsertMany(new[]
            {
                new ProjectEntity
                {
                    Id = _project3AId,
                    Title = Project3ATitle,
                    Description = Project3ADescription,
                    ProjectRoles = new [] {NewProjectRole(user3Entity)},
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test",
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = "Test"
                },
                new ProjectEntity
                {
                    Id = _project3BId,
                    Title = Project3BTitle,
                    Description = Project3BDescription,
                    ProjectRoles = new [] {NewProjectRole(user3Entity)},
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Test",
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = "Test"
                }
            });
            // Save changes
            _unitOfWork.SaveChanges().Wait();
        }
        
        private ProjectRoleEntity NewProjectRole(UserEntity userEntity)
        {
            return new()
            {
                UserEntity = userEntity,
                Role = ProjectRoleValue.OWNER,
                PendingInvite = false
            };
        }

        [Fact]
        public async void CreateProject_AsSignedInUserAndWithRequiredFields_ProjectCanBeQueried()
        {
            _userContextAccessorService.SignInForTesting(User1Id, User1UserName, _user1Permissions);
            var projectSave = new ProjectSaveDto
            {
                Title = "Make App",
                Description = "Make app for users"
            };
            
            var createResult = await _projectsService.CreateNewProject(projectSave);
            var queriedResult = await _projectsService.FindProjectById(createResult.Id);
            
            Assert.Equal(createResult.Id, queriedResult.Project.Id);
            Assert.Equal(projectSave.Title, queriedResult.Project.Title);
            Assert.Equal(projectSave.Description, queriedResult.Project.Description);
            Assert.Equal(User1Id, queriedResult.Project.CreatedBy);
            Assert.Equal(User1Id, queriedResult.Project.ModifiedBy);
            Assert.Equal(User1Id, queriedResult.Owner.UserId);
            Assert.Equal(User1UserName, queriedResult.Owner.UserName);
        }
        
        [Fact]
        public async void FindAnyProjects_SearchForThreeProjectsPageOne_ThreeProjectsFound()
        {
            var pagination = new Pagination
            {
                PageNumber = 0,
                PageSize = 3
            };
            var projects = await _projectsService.FindProjects(pagination, null, null,
                true, true, false);
            Assert.Equal(3, projects.ContentSize);
            Assert.Equal(ProjectsTotal, projects.Total);
            Assert.Equal(0, projects.Pagination.PageNumber);
            Assert.False(projects.Content.IsNullOrEmpty());
        }
        
        [Fact]
        public async void FindProjectById_Project1AIdAsUser1_ReturnProject()
        {
            _userContextAccessorService.SignInForTesting(User1Id, User1UserName, _user1Permissions);
            var projectSearchDto = await _projectsService.FindProjectById(_project1AId);
            Assert.Equal(User1Id, projectSearchDto.Owner.UserId);
            Assert.Equal(User1UserName, projectSearchDto.Owner.UserName);
            Assert.Equal(_project1AId, projectSearchDto.Project.Id);
            Assert.Equal(Project1ATitle, projectSearchDto.Project.Title);
            Assert.Equal(Project1ADescription, projectSearchDto.Project.Description);
       }
        
        [Fact]
        public async void FindProjectById_Project1AIdAsUser2_ThrowForbidden()
        {
            _userContextAccessorService.SignInForTesting(User2Id, User2UserName, _user2Permissions);
            var exception = await Assert.ThrowsAsync<AppException<ErrorCode>>(async () =>
            {
                await _projectsService.FindProjectById(_project1AId);
            });
            Assert.Equal(ErrorCode.NoProjectPermissions, exception.ErrorCode);
            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        }
        
        [Fact]
        public async void FindProjectById_UseIdNotInDatabase_ThrowNotFound()
        {
            var projectId = Guid.Parse("44a8079d-ef92-434c-a3d2-aaa4848c4396");
            var exception = await Assert.ThrowsAsync<AppException<ErrorCode>>(async () =>
            {
                await _projectsService.FindProjectById(projectId, false);
            });
            Assert.Equal(ErrorCode.ProjectIdNotFound, exception.ErrorCode);
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal($"Project with id {projectId.ToString()} not found", exception.Message);
        }
    }
}