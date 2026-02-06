using System.ComponentModel.DataAnnotations;

namespace Skoob.DTOs;

public class CreateUserDTO
{
    [Required(ErrorMessage = "Nome de usuário é obrigatório")]
    [StringLength(15, MinimumLength = 4, ErrorMessage = "Nome de usuário deve ter entre 5 e 15 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Nome de usuário só pode conter letras, números e underscore")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(256, ErrorMessage = "Email não pode ter mais de 256 caracteres")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$", 
        ErrorMessage = "Senha deve conter pelo menos: 1 maiúscula, 1 minúscula, 1 número e 1 caractere especial")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare(nameof(Password), ErrorMessage = "Senha e confirmação não coincidem")]
    public string ConfirmPassword { get; set; } = null!;


}