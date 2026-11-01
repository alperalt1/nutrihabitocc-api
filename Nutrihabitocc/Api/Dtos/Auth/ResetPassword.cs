using System.ComponentModel.DataAnnotations;

namespace Nutrihabitocc.Api.Dtos.Auth;

public class ResetPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;
}