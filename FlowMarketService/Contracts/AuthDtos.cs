using System.ComponentModel.DataAnnotations;

namespace FlowMarketService.Contracts;

/// <summary>
/// Ro‘yxatdan o‘tish: UI dagi "Email / Телефон" rejimiga mos.
/// JSON: camelCase (fullName, mode, acceptTerms, email, password, confirmPassword, phone, handle, referralCode).
/// </summary>
public sealed class RegisterRequest : IValidatableObject
{
    /// <summary>to‘liq ism (Имя)</summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    /// <summary>"email" yoki "phone" (katta-kichik harf farqi yo‘q)</summary>
    [Required]
    public string Mode { get; set; } = "email";

    /// <summary>Oferta / maxfiylik qabul qilinganmi (majburiy)</summary>
    public bool AcceptTerms { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string? Password { get; set; }

    [Required]
    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }

    /// <summary>Telefon rejimi: +998 yoki 9 raqamli mobil format</summary>
    [Phone]
    public string? Phone { get; set; }

    [StringLength(64, MinimumLength = 3)]
    public string? Handle { get; set; }
    public string? ReferralCode { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var mode = (Mode ?? string.Empty).Trim().ToLowerInvariant();
        if (mode is not ("email" or "phone"))
        {
            yield return new ValidationResult("Mode faqat \"email\" yoki \"phone\" bo‘lishi kerak.", [nameof(Mode)]);
            yield break;
        }

        if (!AcceptTerms)
            yield return new ValidationResult("Oferta va maxfiylik siyosatiga rozilik berilishi kerak.", [nameof(AcceptTerms)]);

        if (mode == "email" && string.IsNullOrWhiteSpace(Email))
            yield return new ValidationResult("Email majburiy.", [nameof(Email)]);

        if (mode == "phone" && string.IsNullOrWhiteSpace(Phone))
            yield return new ValidationResult("Telefon raqami majburiy.", [nameof(Phone)]);
    }
}

/// <summary>
/// Kirish: <c>email</c> maydoniga haqiqiy email yoki telefon (+998...) yuborish mumkin.
/// </summary>
public sealed class LoginRequest
{
    [Required]
    public string Email { get; set; } = "";
    [Required]
    public string Password { get; set; } = "";
}

public sealed class LoginByEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
    [Required]
    public string Password { get; set; } = "";
}

public sealed class LoginByPhoneRequest
{
    /// <summary>+998901234567 yoki 901234567</summary>
    [Required]
    [Phone]
    public string Phone { get; set; } = "";
    [Required]
    public string Password { get; set; } = "";
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = "";
    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string NewPassword { get; set; } = "";
    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = "";
}

public sealed class AdminResetPasswordRequest
{
    /// <summary>UserId, email yoki phone (+998...).</summary>
    [Required]
    public string UserIdentifier { get; set; } = "";

    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string NewPassword { get; set; } = "";

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = "";
}

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresUtc,
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles);

public record RefreshTokenRequest(string RefreshToken);

/// <summary>POST /api/auth/register/email</summary>
public sealed class RegisterByEmailRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = "";
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = "";
    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
    public bool AcceptTerms { get; set; }
    [StringLength(64, MinimumLength = 3)]
    public string? Handle { get; set; }
    public string? ReferralCode { get; set; }
}

/// <summary>POST /api/auth/register/phone</summary>
public sealed class RegisterByPhoneRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = "";
    /// <summary>+998901234567 yoki 901234567</summary>
    [Required]
    [Phone]
    public string Phone { get; set; } = "";
    [Required]
    [StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = "";
    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
    public bool AcceptTerms { get; set; }
    [StringLength(64, MinimumLength = 3)]
    public string? Handle { get; set; }
    public string? ReferralCode { get; set; }
}
