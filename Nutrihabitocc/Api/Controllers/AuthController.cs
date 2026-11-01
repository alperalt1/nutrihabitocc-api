using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nutrihabitocc.Api.Dtos;
using Nutrihabitocc.Api.Dtos.Auth;
using Nutrihabitocc.Domain.Constants;
using Nutrihabitocc.Domain.Entities;
using Nutrihabitocc.Domain.Exceptions;
using Nutrihabitocc.Domain.Ports.Inputs;
using Nutrihabitocc.Domain.Ports.Outputs;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserService userService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [Authorize(Policy = AuthPolicies.WebStaffOnly)]
    [HttpPost("crearusuario")]
    public async Task<ActionResult<ApiResponse<string>>> CreateUserAsync([FromBody] RegisterRequestDto request)
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
            
            await _userService.CreateUserAsync(userEntity, request.Password, request.IdRol);
            return Ok(ApiResponse<string>.Ok("Usuario creado exitosamente."));
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
            _logger.LogError(ex, "Error al crear el usuario.");
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginUserAsync([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request.Email, request.Password, request.Platform);
            var dataUser = new LoginDataUser
            {
                Id = response.User.Id,
                Nombres = response.User.Nombres,
                Apellidos = response.User.Apellidos,
                Email = response.User.Email,
                Phone = response.User.Phone,
                IsActive = response.User.IsActive
            };
            var dtobody = new LoginResponseDto
            {
                User = dataUser,
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                Platform = response.Platform,
                Roles = response.Roles
            };
            return Ok(ApiResponse<LoginResponseDto>.Ok(dtobody));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión.");
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshTokenAsync(
        [FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            var dataUser = new LoginDataUser
            {
                Id = response.User.Id,
                Nombres = response.User.Nombres,
                Apellidos = response.User.Apellidos,
                Email = response.User.Email,
                Phone = response.User.Phone,
                IsActive = response.User.IsActive
            };
            var dtobody = new LoginResponseDto
            {
                User = dataUser,
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                Platform = response.Platform,
                Roles = response.Roles
            };
            return Ok(ApiResponse<LoginResponseDto>.Ok(dtobody));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(403, ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al refrescar el token.");
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }));
        }
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResponse<MeResponseDto>> Me()
    {
        try
        {
            var currentUser = _currentUserService.GetCurrentUser();
            return Ok(ApiResponse<MeResponseDto>.Ok(new MeResponseDto
            {
                Id = currentUser.UserId,
                Email = currentUser.Email,
                Platform = currentUser.Platform,
                Roles = currentUser.Roles.ToList()
            }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<MeResponseDto>.Fail(ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            await _authService.GenerarCorreoRecuperacionAsync(request.Email);
            return Ok(ApiResponse<string>.Ok("Si el correo está registrado, recibirás un código de verificación."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el correo de recuperación.");
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno en el servidor.",
                new List<string> { ex.Message }
            ));
        }
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var success = await _authService.ResetPasswordWithOtpAsync(request.Email, request.Code, request.NewPassword);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.Fail("El código de verificación es inválido, ha expirado o el usuario no existe."));
            }
            return Ok(ApiResponse<string>.Ok("Contraseña actualizada exitosamente."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la contraseña.");
            return StatusCode(500, ApiResponse<string>.Fail(
                "Ocurrió un error interno al actualizar la contraseña.",
                new List<string> { ex.Message }));
        }
    }
}
