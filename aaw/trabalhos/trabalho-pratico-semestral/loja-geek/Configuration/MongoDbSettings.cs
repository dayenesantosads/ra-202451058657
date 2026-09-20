namespace loja_geek.Configuration;

public class MongoDbSettings {

    public string ConnectionString {get; set; } = string.Empty; // guarda o endereço do MongoDB
    public string DatabaseName {get; set; } = string.Empty; // guarda o nome do banco que Loja Geek vai usar

}