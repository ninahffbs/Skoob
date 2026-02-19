using System.ComponentModel.DataAnnotations;

namespace Skoob.DTOs;

public class UpdateReviewDTO
{
    [Required, MaxLength(500, ErrorMessage = "O review não pode ultrapassar 500 caracteres.")]
    public string? ReviewText { get; set; }
}