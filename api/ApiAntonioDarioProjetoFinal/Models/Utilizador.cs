namespace ApiAntonioDarioProjetoFinal.Models;

public class Utilizador
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "User";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}