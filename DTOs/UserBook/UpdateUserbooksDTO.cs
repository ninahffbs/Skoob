using System.ComponentModel.DataAnnotations;
using Skoob.Enums;

namespace Skoob.DTOs;

public class UpdateUserbooksDTO
{
     [Range(0, int.MaxValue, ErrorMessage = "Páginas lidas deve ser >= 0")]
    public int? PagesRead { get; set; }

    [Required(ErrorMessage = "Status é obrigatório")]    
    public StatusBook Status { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? FinishDate { get; set; }
    
    [Range(1, 5, ErrorMessage = "Rating deve ser entre 1 e 5")]
    public int? Rating { get; set; }
    
    [MaxLength(500, ErrorMessage = "Review não pode ter mais de 500 caracteres")]
    public string? Review { get; set; }
}