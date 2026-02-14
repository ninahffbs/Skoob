using System.ComponentModel.DataAnnotations;
public class AddRatingDTO
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating deve ser entre 1 e 5")]
    public int? Rating { get; set; }
}