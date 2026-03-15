using System.Security.Claims;
using Friendout.Domain.DTOs.Equipment;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller pour la gestion de l'équipement des utilisateurs
/// </summary>
[ApiController]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentController"/> class.
    /// </summary>
    /// <param name="equipmentService">Service de gestion de l'équipement</param>
    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    /// <summary>
    /// Récupère l'équipement de l'utilisateur pour une activité donnée
    /// </summary>
    /// <param name="activityId">ID de l'activité</param>
    /// <returns>Liste des équipements avec leur statut de possession par l'utilisateur</returns>
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
    ///  Met à jour l'équipement de l'utilisateur pour une activité
    /// </summary>
    /// <param name="request">Contient les informations ativityId, equipementId et la quantité</param>
    /// <param name="activityId">L'id de l'activité concernée</param>
    /// <returns></returns>
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
