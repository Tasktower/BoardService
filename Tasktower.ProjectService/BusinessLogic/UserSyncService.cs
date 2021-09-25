using System.Threading.Tasks;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Tasktower.ProjectService.DataAccess.Entities;
using Tasktower.ProjectService.DataAccess.Repositories;
using Tasktower.ProjectService.Dtos;
using Tasktower.ProjectService.Dtos.External;

namespace Tasktower.ProjectService.BusinessLogic
{
    public class UserSyncService : IUserSyncService
    {
        private readonly ILogger<UserSyncService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public UserSyncService(ILogger<UserSyncService> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserReadDto> UpdateUserFromExternal(ExtUserSyncMessageDto extUserSync)
        {
            _logger.LogInformation("User with original id {OriginalId} before update = {@User}", 
                extUserSync.UserIdCurrent(), extUserSync.UserProfileToSync);
            UserEntity userEntityToSave;
            var existingUserEntity = await _unitOfWork.UserRepository.GetById(extUserSync.UserIdCurrent());
            if (existingUserEntity == null)
            {
                userEntityToSave = _mapper.Map<UserEntity>(extUserSync.UserProfileToSync);
                userEntityToSave.UpdateAuditProperties("SYSTEM", false);
                await _unitOfWork.UserRepository.Insert(userEntityToSave);
            }
            else
            {
                userEntityToSave = _mapper.Map(extUserSync.UserProfileToSync, existingUserEntity);
                userEntityToSave.UpdateAuditProperties("SYSTEM", false);
                await _unitOfWork.UserRepository.Update(userEntityToSave);
            }
            await _unitOfWork.SaveChanges();

            var userReadDto = _mapper.Map<UserReadDto>(userEntityToSave);
            _logger.LogInformation("User with original id {OriginalId} after update = {@User}", 
                extUserSync.UserIdCurrent(), userReadDto);
            return userReadDto;
        }

        public async Task DeleteUserFromExternal(ExtUserSyncMessageDto extUserSync)
        {
            var existingUserEntity = await _unitOfWork.UserRepository.GetById(extUserSync.UserIdCurrent());
            if (existingUserEntity == null)
            {
                _logger.LogError("Could not find user {@User}", extUserSync);
            }
            else
            {
                await _unitOfWork.UserRepository.Delete(existingUserEntity);
            }
            await _unitOfWork.SaveChanges();
            _logger.LogInformation("deleted user {@User}", extUserSync);
        }
    }
}