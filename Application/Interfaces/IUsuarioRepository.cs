// =============================================================================
// CAMADA: Application (Regras de negócio — depende apenas do Domain)
// ARQUIVO: IUsuarioRepository.cs
// =============================================================================
// CONCEITO — Repository Pattern:
// O Repository é um padrão que esconde os detalhes de como os dados são
// persistidos. A Application sabe que "existe um lugar para buscar e salvar
// usuários", mas não sabe se é SQLite, PostgreSQL, um arquivo JSON, ou uma API.
//
// Este arquivo define O QUE pode ser feito com usuários.
// O SqliteRepository.cs (na Infrastructure) define COMO é feito.
//
// Esse desacoplamento é o que permite trocar o banco de dados no futuro
// sem alterar nenhuma linha de código nas camadas Application e Domain.
// =============================================================================

using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUsuarioRepository
    {
        // Retorna todos os usuários cadastrados no sistema.
        // O "?" em "Usuario?" indica que cada item da lista pode ser nulo
        // (herança do EF Core ao lidar com referências nullable).
        Task<IEnumerable<Usuario?>> ObterUsuariosAsync();

        // Busca um usuário pelo nome de login (case-insensitive na implementação).
        // Retorna null se o usuário não existir — por isso o "?" no retorno.
        Task<Usuario?> ObterPorUsernameAsync(string nome);

        // Persiste um novo usuário no banco de dados.
        Task CadastrarUsuarioAsync(Usuario usuario);

        // Remove um usuário pelo nome. Útil para uma futura tela de gerenciamento.
        Task ExcluirUsuarioAsync(string nome);

        // Verificação rápida: existe QUALQUER usuário cadastrado?
        // Usado na inicialização para decidir se precisa criar o admin padrão.
        Task<bool> ExisteAlgumUsuarioAsync();
    }
}
