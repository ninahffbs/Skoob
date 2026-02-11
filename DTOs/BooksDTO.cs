using System.ComponentModel.DataAnnotations;

namespace Skoob.DTOs;

public class BookDTO
{ 
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public int Pages { get; set; }

    [MaxLength(500, ErrorMessage = "Sinopse não pode ter mais de 500 caracteres")]
    public string? Synopsis { get; set; }
    public int PublishedDate { get; set; }

    // falta continuar adicionar author, genres 
}