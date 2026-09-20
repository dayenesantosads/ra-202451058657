namespace loja_geek.Models;

public class Produto
{
  public string Id { get; set; }
  public string Nome { get; set; }
  public string Descricao { get; set; }
  public string Categoria { get; set; }
  public decimal Preco { get; set; }
  public int Estoque { get; set; }
  public string ImagemUrl { get; set; }
  public DateTime CriadoEm { get; set; }

}