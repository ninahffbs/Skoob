using System.ComponentModel.DataAnnotations;

public class UpdateReadPagesDTO
{
    [Required]
    [Range(0, int.MaxValue)]
    public int PagesRead { get; set; }
}