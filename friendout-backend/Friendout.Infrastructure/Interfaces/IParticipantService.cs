using System.Threading.Tasks;
using Friendout.Domain.DTOs.Participant;
using Friendout.Infrastructure.Command.Participant;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

public interface IParticipantService
{
    public Task<ServiceResult<UserActivityParticipantsDto>> GetActivityParticipantsAsync(string activityId);
    
    public Task<ServiceResult<UserActivityParticipationDto>> SaveParticipationAsync(UpdateParticipationCommand command, string userId);
}