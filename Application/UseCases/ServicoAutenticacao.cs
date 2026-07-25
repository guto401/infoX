using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases;

public class ServicoAutenticacao
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _iPasswordHasher;

    public ServicoAutenticacao(IUsuarioRepository usuarioRepository, IPasswordHasher iPasswordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _iPasswordHasher = iPasswordHasher;
    }

    public async Task<bool> LoginAsync(string username, string senha)
    {
        await GarantirUsuarioPadraoAsync();

        var usuario = await _usuarioRepository.ObterPorUsernameAsync(username);

        if (usuario == null) return false;

        return _iPasswordHasher.VerificarSenha(senha, usuario.PasswordHash);
    }

    public async Task CadastroUsuarioAsync(CadastroUsuarioDto dto)
    {
        var hash = _iPasswordHasher.GerarHash(dto.Senha);
        var novoUsuario = new Usuario
        {
            Nome = dto.Nome,
            PasswordHash = hash
        };
        await _usuarioRepository.CadastrarUsuarioAsync(novoUsuario);
    }

    private async Task GarantirUsuarioPadraoAsync()
    {
        if (!await _usuarioRepository.ExisteAlgumUsuarioAsync())
        {
            await CadastroUsuarioAsync(new CadastroUsuarioDto { Nome = "admin", Senha = "1234" });
        }
    }
}