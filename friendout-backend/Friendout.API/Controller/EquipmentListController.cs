using System.Security.Claims;
using Friendout.Domain.DTOs.EquipmentList;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller for managing the current user's reusable equipment lists.
/// </summary>
[ApiController]
[Authorize]
[Route("equipment-lists")]
public class EquipmentListController : ControllerBase
{
    private readonly IEquipmentListService _equipmentListService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentListController"/> class.
    /// </summary>
    /// <param name="equipmentListService">Service responsible for equipment list operations.</param>
    public EquipmentListController(IEquipmentListService equipmentListService)
    {
        _equipmentListService = equipmentListService;
    }

    /// <summary>
    /// Returns all equipment lists owned by the current user.
    /// </summary>
    [ProducesResponseType(typeof(List<EquipmentListDto>), 200)]
    [ProducesResponseType(401)]
    [HttpGet]
    public async Task<IActionResult> GetUserEquipmentLists()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _equipmentListService.GetUserEquipmentListsAsync(userId);

        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Returns a single equipment list owned by the current user.
    /// </summary>
    /// <param name="id">The equipment list ID.</param>
    [ProducesResponseType(typeof(EquipmentListDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEquipmentListById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _equipmentListService.GetEquipmentListByIdAsync(id, userId);

        if (result.IsSuccess)
            return Ok(result.Data);

        return NotFound(result.ErrorMessage);
    }

    /// <summary>
    /// Creates a new equipment list for the current user.
    /// </summary>
    /// <param name="request">The list name and items.</param>
    [ProducesResponseType(typeof(EquipmentListDto), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    [HttpPost]
    public async Task<IActionResult> CreateEquipmentList([FromBody] CreateEquipmentListDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _equipmentListService.CreateEquipmentListAsync(userId, request);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetEquipmentListById), new { id = result.Data!.Id }, result.Data);

        return BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Updates an existing equipment list owned by the current user. Items are fully replaced.
    /// </summary>
    /// <param name="id">The equipment list ID.</param>
    /// <param name="request">The new name and items.</param>
    [ProducesResponseType(typeof(EquipmentListDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEquipmentList(string id, [FromBody] UpdateEquipmentListDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _equipmentListService.UpdateEquipmentListAsync(id, userId, request);

        if (result.IsSuccess)
            return Ok(result.Data);

        return result.ErrorMessage == "Equipment list not found"
            ? NotFound(result.ErrorMessage)
            : BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Deletes an equipment list owned by the current user.
    /// </summary>
    /// <param name="id">The equipment list ID.</param>
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEquipmentList(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _equipmentListService.DeleteEquipmentListAsync(id, userId);

        if (result.IsSuccess)
            return NoContent();

        return NotFound(result.ErrorMessage);
    }
}
