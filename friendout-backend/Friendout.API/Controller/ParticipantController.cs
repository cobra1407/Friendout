using System.Security.Claims;
using Friendout.Domain.DTOs.Participant;
using Friendout.Infrastructure.Command.Participant;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;
/// <summary>
/// Participant controller
/// </summary>
[ApiController]
public class ParticipantController : ControllerBase
{
    /// <summary>
    ///  Participant service
    /// </summary>
    private IParticipantService _participantService;
    
    public ParticipantController(IParticipantService participantService)
    {
        _participantService = participantService;
    }


    [Authorize]
    [ProducesResponseType(typeof(List<ParticipantDto>), 200)]
    [ProducesResponseType(401)]
    [HttpGet("activities/{activityId}/participants")]
    public async Task<IActionResult> GetActivityParticipants([FromRoute] string activityId)
    {

        if (string.IsNullOrEmpty(activityId))
            return BadRequest("ActivityId required");
        
        var restult = await _participantService.GetActivityParticipantsAsync(activityId);

        if (restult.IsSuccess)
        {
            return Ok(restult.Data);
        }
        
        return BadRequest(restult.ErrorMessage);
        
    }


    [Authorize]
    [HttpPut("activities/{activityId}/participation")]
    [ProducesResponseType(typeof(ParticipantDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpsertParticipation(
        [FromRoute] string activityId, 
        [FromBody] UpdateParticipationDto dto)
    {
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");
        
        var result = await _participantService. SaveParticipationAsync(new UpdateParticipationCommand
        {
            Status = dto.Status,
            ActivityId = activityId,
            SubActivityIds = dto.SubActivityIds
        }, userId);
        
        if(result.IsSuccess)
            return Ok(result.Data);
        
        return BadRequest(result.ErrorMessage);
    }

}