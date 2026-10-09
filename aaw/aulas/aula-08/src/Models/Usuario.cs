namespace BibliotecaApi.Models;

public record Usuario(int Id, string Nome, string Email, string Senha, string Role);

public record LoginRequest(string Email, string Senha);
