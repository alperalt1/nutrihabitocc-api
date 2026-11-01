using System.ComponentModel.DataAnnotations;

namespace Nutrihabitocc.Api.Dtos.Auth;

public class ForgotPasswordRequestDto
{
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
}