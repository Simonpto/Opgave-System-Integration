using Microsoft.AspNetCore.Mvc;
using UserProfile.Application.UserProfiles;
using UserProfile.Presentation.Contracts;

namespace UserProfile.Presentation.Controllers;

/// <summary>HTTP mapping only. Errors are turned into status codes by <see cref="ErrorHandling.ExceptionToProblemDetailsHandler"/>.</summary>
[ApiController]
[Route("api/userprofiles")]
[Produces("application/json")]
public class UserProfilesController : ControllerBase
{
    private readonly IUserProfileService _service;

    public UserProfilesController(IUserProfileService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileResponse>> Create(CreateUserProfileRequest request, CancellationToken ct)
    {
        var profile = await _service.CreateAsync(
            new CreateUserProfileCommand(request.Username, request.DisplayName, request.Email, request.Bio, request.AvatarUrl), ct);

        return CreatedAtAction(nameof(GetById), new { id = profile.Id }, UserProfileResponse.From(profile));
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<UserProfileResponse>>(StatusCodes.Status200OK)]
    public async Task<IEnumerable<UserProfileResponse>> GetAll(CancellationToken ct)
    {
        var profiles = await _service.GetAllAsync(ct);
        return profiles.Select(UserProfileResponse.From);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<UserProfileResponse> GetById(Guid id, CancellationToken ct)
    {
        return UserProfileResponse.From(await _service.GetByIdAsync(id, ct));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<UserProfileResponse> Update(Guid id, UpdateUserProfileRequest request, CancellationToken ct)
    {
        var profile = await _service.UpdateAsync(
            id, new UpdateUserProfileCommand(request.DisplayName, request.Bio, request.AvatarUrl), ct);

        return UserProfileResponse.From(profile);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
