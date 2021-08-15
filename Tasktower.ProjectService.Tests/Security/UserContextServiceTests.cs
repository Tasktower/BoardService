using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Tasktower.Lib.Aspnetcore.Security;
using Tasktower.Lib.Aspnetcore.Services;
using Tasktower.Lib.Aspnetcore.Services.Contexts;
using Tasktower.ProjectService.Tests.TestTools.Helpers;
using Xunit;

namespace Tasktower.ProjectService.Tests.Security
{
    public sealed class UserContextServiceTests : IDisposable
    {
        private readonly IUserContextAccessorService _userContextAccessorService;

        public UserContextServiceTests(IUserContextAccessorService userContextAccessorService)
        {
            _userContextAccessorService = userContextAccessorService;
        }
        
        public void Dispose()
        {
            _userContextAccessorService.SignOutForTesting();
        }

        [Fact]
        public void UserContext_UserAuthenticated_ReturnAuthenticatedUserInfo()
        {
            const string userId = "123";
            const string name = "John Stewart";
            var permissions = new HashSet<string>() {SecurityGroups.TasktowerAdmin};
            _userContextAccessorService.SignInForTesting(userId, name, permissions);
            Assert.True(_userContextAccessorService.UserContext.IsAuthenticated);
            Assert.Equal(userId, _userContextAccessorService.UserContext.UserId);
            Assert.Equal(name, _userContextAccessorService.UserContext.Name);
            Assert.Collection(_userContextAccessorService.UserContext.Permissions, 
                p => Assert.Equal(p, SecurityGroups.TasktowerAdmin));
        }
        
        [Fact]
        public void UserContext_NoUserAuthenticated_ReturnAnonymousUser()
        {
            Assert.Equal("ANONYMOUS", _userContextAccessorService.UserContext.Name);
            Assert.Equal("ANONYMOUS", _userContextAccessorService.UserContext.UserId);
            Assert.False(_userContextAccessorService.UserContext.IsAuthenticated);
            Assert.Equal(0, _userContextAccessorService.UserContext.Permissions.Count);
        }

        [Fact] public void CreateUserContext_WithAuthenticatedPrinciple_ReturnAnonymousUser()
        {
            const string userId = "123";
            const string name = "John Stewart";
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new(ClaimTypes.NameIdentifier, userId),
                        new(ClaimTypes.Name, name),
                        new(UserContextAccessorService.PermissionsClaim, SecurityGroups.TasktowerAdmin),
                    },
                    "Basic")
            );
            
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.User).Returns(claimsPrincipal);
        
            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = httpContextMock.Object;
        
            var testUserContext = new UserContextAccessorService(httpContextAccessor);
        
            Assert.True(testUserContext.UserContext.IsAuthenticated);
            Assert.Equal(userId, testUserContext.UserContext.UserId);
            Assert.Equal(name, testUserContext.UserContext.Name);
            Assert.Collection(testUserContext.UserContext.Permissions, 
                p => Assert.Equal(p, SecurityGroups.TasktowerAdmin));
        }

        [Fact]
        public void CreateUserContext_WithEmptyPrinciple_ReturnAnonymousUser()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.User).Returns(claimsPrincipal);

            var httpContextAccessor = new HttpContextAccessor();
            httpContextAccessor.HttpContext = httpContextMock.Object;

            var testUserContext = new UserContextAccessorService(httpContextAccessor);

            Assert.False(claimsPrincipal.Identity?.IsAuthenticated);
            Assert.Equal("ANONYMOUS", testUserContext.UserContext.Name);
            Assert.Equal("ANONYMOUS", testUserContext.UserContext.UserId);
            Assert.False(testUserContext.UserContext.IsAuthenticated);
            Assert.Equal(0, testUserContext.UserContext.Permissions.Count);
        }
    }
}