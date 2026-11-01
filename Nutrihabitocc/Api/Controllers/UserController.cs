using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Api.Dtos.Auth;
using Nutrihabitocc.Api.Dtos.User;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Dtos;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController: ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    
    public UserController(ILogger<UserController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<ICollection<RoleResponseDto>>>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _userService.GetAllRolesAsync();
            var rolesResponse = roles.Select(role => new RoleResponseDto
            {
                Id = role.Id,
                Name = role.RoleName
            }).ToList();

            return Ok(ApiResponse<ICollection<RoleResponseDto>>.Ok(rolesResponse));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<RoleResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los roles.");
            return StatusCode(500, ApiResponse<ICollection<RoleResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ICollection<UserResponseDto>>>> GetUsersAsync()
    {
        try
        {
            var users = await _userService.GetUsersAsync();
            var response = users.Select(Map).ToList();
            return Ok(ApiResponse<ICollection<UserResponseDto>>.Ok(response));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<ICollection<UserResponseDto>>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los usuarios.");
            return StatusCode(500, ApiResponse<ICollection<UserResponseDto>>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpPost("crear")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUserAsync(
        [FromBody] RegisterRequestDto request)
    {
        try
        {
            var userEntity = new User
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone,
            };

            var created = await _userService.CreateUserAsync(userEntity, request.Password, request.IdRol);
            return Ok(ApiResponse<UserResponseDto>.Ok(
                Map(created),
                "Usuario creado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<UserResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el usuario.");
            return StatusCode(500, ApiResponse<UserResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [Authorize(Policy = AuthPolicies.AdminWebOnly)]
    [HttpPut("{id:int}/roles")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateUserRolesAsync(
        int id,
        [FromBody] UpdateUserRolesRequestDto request)
    {
        try
        {
            var updated = await _userService.UpdateUserRolesAsync(id, request.RoleIds);
            if (!updated)
            {
                return NotFound(ApiResponse<string>.Fail("El usuario no existe o está inactivo."));
            }

            return Ok(ApiResponse<string>.Ok("Roles del usuario actualizados exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar los roles del usuario {Id}.", id);
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPost("crearperfil")]
    public async Task<ActionResult<ApiResponse<string>>> CrearPerfilAsync([FromBody] PerfilRequestDto dto)
    {
        try
        {
            var profileEntity = new Profile
            {
                Height = dto.Height,
                Weight = dto.Weight,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                GrasaCorporal = dto.GrasaCorporal,
                MasaMuscular = dto.MasaMuscular,
                UserId = dto.UserId,
            };
            await _userService.CreateProfileAsync(profileEntity);
            return Ok(ApiResponse<string>.Ok("Perfil creado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el perfil.");
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [HttpPut("modificarperfil")]
    public async Task<ActionResult<ApiResponse<string>>> ModificarPerfilAsync([FromBody] PerfilRequestDto dto)
    {
        try
        {
            var profileEntity = new Profile
            {
                Id = dto.PerfilId,
                Height = dto.Height,
                Weight = dto.Weight,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                GrasaCorporal = dto.GrasaCorporal,
                MasaMuscular = dto.MasaMuscular,
                UserId = dto.UserId,
            };
            await _userService.EditProfileAsync(profileEntity);
            return Ok(ApiResponse<string>.Ok("Perfil editado exitosamente."));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al modificar el perfil.");
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    private static UserResponseDto Map(UserResult user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Nombres = user.Nombres,
            Apellidos = user.Apellidos,
            Email = user.Email,
            Phone = user.Phone,
            IsActive = user.IsActive,
            Roles = user.Roles,
            CreatedOn = user.CreatedOn
        };
    }
}
