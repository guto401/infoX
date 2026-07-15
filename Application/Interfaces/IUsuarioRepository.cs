using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> ObterUsuariosAsync(string username);
        Task<Usuario?> ObterPorUsernameAsync(string username);
        Task CadastrarUsuarioAsync(Usuario usuario);
        Task ExcluirUsuarioAsync(string username);
        Task<bool> ExisteAlgumUsuarioAsync();
    }
}
