using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tasktower.Lib.Aspnetcore.DataAccess.Repositories;
using Tasktower.ProjectService.DataAccess.Entities;
using Tasktower.ProjectService.Tools.Constants;

namespace Tasktower.ProjectService.DataAccess.Repositories
{
    public interface IProjectRoleRepository : ICrudRepository<Guid, ProjectRoleEntity>
    {
        /// <summary>
        /// Returns a value task indicating if the user has project permissions
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="projectRoles">SecurityGroups the user is tested against to see if any match</param>
        /// <returns></returns>
        Task<bool> UserHasProjectRolePermission(Guid projectId, string userId, 
            ISet<ProjectRoleValue> projectRoles, bool allowPendingInvite = false);

        Task<ProjectRoleEntity> findProjectOwner(Guid projectId);
    }
}
