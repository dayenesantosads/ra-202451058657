namespace loja_geek.Models;

public class Produto
{
  public string id { get; set; }
  public string nome { get; set; }
  public string descricao { get; set; }
  public string categoria { get; set; }
  public decimal preco { get; set; }
  public int estoque { get; set; }
  public string imagemUrl { get; set; }
  public DateTime criadoEm { get; set; }

}