using System.ComponentModel.DataAnnotations;
namespace Skoob.DTOs;

public class UpdatePasswordDTO
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
        ErrorMessage = "Senha deve conter pelo menos: 1 maiúscula, 1 minúscula, 1 número e 1 caractere especial")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare(nameof(NewPassword), ErrorMessage = "Senha e confirmação não coincidem")]
    public string ConfirmNewPassword { get; set; } = null!;
}
