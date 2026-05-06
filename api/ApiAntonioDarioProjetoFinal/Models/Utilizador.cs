using System.ComponentModel.DataAnnotations;

namespace ApiAntonioDarioProjetoFinal.Models;

public class Utilizador
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "User";

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}