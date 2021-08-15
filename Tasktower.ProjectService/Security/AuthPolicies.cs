using System.Collections.Generic;
using Tasktower.Lib.Aspnetcore.Security;

namespace Tasktower.ProjectService.Security
{
    public static class AuthPolicies
    {
        public static IEnumerable<AuthPolicy> Get()
        {
            return new List<AuthPolicy>()
            {
                new()
                {
                    Name = AuthPolicyNames.ReadProjectsAny,
                    SecurityGroups = new[] {SecurityGroups.TasktowerAdmin}
                },
                new() 
                {
                    Name = AuthPolicyNames.UpdateProjectsAny,
                    SecurityGroups = new[] {SecurityGroups.TasktowerAdmin}
                },
                new()
                {
                    Name = AuthPolicyNames.DeleteProjectsAny,
                    SecurityGroups = new[] {SecurityGroups.TasktowerAdmin}
                },
            };
        }
    }
}