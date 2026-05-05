namespace ApiAntonioDarioProjetoFinal.Models;

public class Produto
{
    public int      Id        { get; set; }
    public string   Nome      { get; set; } = "";
    public string?  Descricao { get; set; }
    public decimal  Preco     { get; set; }
    public int      Stock     { get; set; }
    public string   SKU       { get; set; } = "";
    public DateTime CriadoEm  { get; set; } = DateTime.UtcNow;
}