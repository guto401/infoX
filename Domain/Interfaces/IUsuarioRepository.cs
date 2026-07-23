using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario?>> ObterUsuariosAsync();

        Task<Usuario?> ObterPorUsernameAsync(string nome);

        Task CadastrarUsuarioAsync(Usuario usuario);

        Task ExcluirUsuarioAsync(string nome);

        Task<bool> ExisteAlgumUsuarioAsync();
    }
}
