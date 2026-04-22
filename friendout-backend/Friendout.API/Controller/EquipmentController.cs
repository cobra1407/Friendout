using System.Security.Claims;
using Friendout.Domain.DTOs.Equipment;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller for managing user equipment on activities.
/// </summary>
[ApiController]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentController"/> class.
    /// </summary>
    /// <param name="equipmentService">Service responsible for equipment operations.</param>
    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    /// <summary>
    /// Returns the equipment list for a given activity, including ownership status for the current user.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <returns>List of equipment items with the user's ownership status.</returns>
    [Authorize]
    [ProducesResponseType(typeof(List<UserEquipmentDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    [HttpGet("activities/{activityId}/user/equipment")]
    public async Task<IActionResult> GetUserEquipmentForActivity(string activityId)
    {
         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var result = await _equipmentService.GetUserEquipmentForActivityAsync(activityId, userId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Updates the equipment ownership for the current user on a given activity.
    /// </summary>
    /// <param name="request">Contains the equipmentId and quantity to set.</param>
    /// <param name="activityId">The activity ID.</param>
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    [HttpPut("activities/{activityId}/user/equipment")]
    public async Task<IActionResult> SetUserEquipment([FromBody] SetUserEquipmentDto request, [FromRoute] string activityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(activityId))
            return BadRequest("ActivityId is required.");
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");
        
        
        var result = await _equipmentService.SetUserEquipmentAsync(
            activityId,
            request.EquipmentId, 
            userId, 
            request.Quantity);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.ErrorMessage);
    }
    
}
