using Application.Security;
using Domain.Entities;
using Application.Interfaces;

namespace Application.UseCases;

public class ServicoAutenticacao
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ServicoAutenticacao(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<bool> LoginAsync(string username, string senha)
    {
        // Garante a existência de um usuário padrão caso o SQLite esteja vazio
        await GarantirUsuarioPadraoAsync();

        var usuario = await _usuarioRepository.ObterPorUsernameAsync(username);
        if (usuario == null) return false;

        return Argon2Helper.VerificarSenha(senha, usuario.PasswordHash);
    }

    private async Task GarantirUsuarioPadraoAsync()
    {
        if (!await _usuarioRepository.ExisteAlgumUsuarioAsync())
        {
            var senhaHash = Argon2Helper.GerarHash("admin");
            var usuarioPadrao = new Usuario("admin", senhaHash);
            await _usuarioRepository.CadastrarUsuarioAsync(usuarioPadrao);
        }
    }
}