// =============================================================================
// CAMADA: Domain (O núcleo da cebola — não depende de NADA externo)
// ARQUIVO: Usuario.cs
// =============================================================================
// Uma "Entidade" no DDD (Domain-Driven Design) é um objeto que tem identidade
// própria, representada pelo campo Id. Dois usuários com o mesmo nome mas Ids
// diferentes são considerados objetos distintos.
//
// Note que esta classe não sabe NADA sobre banco de dados, PowerShell, ou UI.
// Ela representa apenas o conceito puro de "um usuário do sistema".
// Quem cuida da persistência é a Infrastructure. Quem cuida das regras de
// negócio (ex: o login) é a Application.
// =============================================================================

namespace Domain.Entities
{
    public class Usuario
    {
        // Chave primária. O EF Core reconhece "Id" por convenção e gera
        // o valor automaticamente (auto-increment no SQLite).
        public int Id { get; set; }

        // O nome de login do usuário. Mapeado com índice UNIQUE no banco
        // (veja AppDbContext.cs) para garantir que não existam duplicatas.
        public string Nome { get; set; } = string.Empty;

        // NUNCA guardamos a senha em texto puro. Guardamos apenas o hash
        // gerado pelo Argon2id (veja Argon2Helper.cs).
        public string PasswordHash { get; set; } = string.Empty;

        // Construtor vazio exigido pelo Entity Framework Core.
        // O EF precisa instanciar o objeto via reflexão ao ler do banco,
        // e para isso ele usa o construtor sem parâmetros.
        public Usuario() { }

        // Construtor de conveniência para criar um usuário já preenchido.
        public Usuario(string nome, string passwordHash)
        {
            Nome = nome;         
            PasswordHash = passwordHash;
        }
    }
}
