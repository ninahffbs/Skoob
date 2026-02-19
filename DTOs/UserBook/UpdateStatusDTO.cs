using System.ComponentModel.DataAnnotations;
using Skoob.Enums;

namespace Skoob.DTOs;

public class UpdateStatusDTO
{
    [Required(ErrorMessage = "O status é obrigatório")]
    [EnumDataType(typeof(StatusBook), ErrorMessage = "Status inválido! Valores aceitos: 0 (Lendo), 1 (Lido), 2 (QueroLer)")]
    public StatusBook Status { get; set; }
}