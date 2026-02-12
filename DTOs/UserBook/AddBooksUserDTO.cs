using System.ComponentModel.DataAnnotations;
using Skoob.Enums;

namespace Skoob.DTOs;

public class AddBooksUserDTO
{   
    public Guid BookId { get; set; }

    [Required(ErrorMessage = "Status é obrigatório")]
    public StatusBook Status { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Páginas lidas deve ser maior ou igual a 0")]
    public int PagesRead { get; set; } = 0;
    public DateTime? StartDate { get; set; }
}