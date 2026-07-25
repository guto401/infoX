// =============================================================================
// CAMADA: Application (Regras de negócio — depende apenas do Domain)
// ARQUIVO: ServicoAutenticacao.cs
// =============================================================================
// CONCEITO — Use Case / Application Service:
// Um "Use Case" encapsula uma operação de negócio específica e orquestra
// as dependências necessárias para realizá-la. Ele é o "diretor de orquestra":
// sabe QUEM chamar e em QUE ORDEM, mas não sabe os detalhes de implementação.
//
// O ServicoAutenticacao cuida de tudo relacionado a autenticação:
// - Verificar se existe algum usuário (e criar o admin padrão se não houver)
// - Validar credenciais comparando a senha digitada com o hash no banco
//
// Ele depende de abstrações (IUsuarioRepository), nunca de implementações
// concretas — isso é o princípio D do SOLID (Dependency Inversion Principle).
// =============================================================================

using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.UseCases;

public class ServicoAutenticacao
{
    // Dependência declarada como a INTERFACE, não como a classe concreta.
    // Isso significa que este serviço funciona com qualquer implementação:
    // SqliteRepository, um repositório em memória para testes, ou um futuro
    // repositório que consulte uma API remota.
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _iPasswordHasher;

    // CONCEITO — Injeção de Dependência (Constructor Injection):
    // Em vez de criar o repositório aqui com "new SqliteRepository()",
    // recebemos ele pronto pelo construtor. Quem "injeta" a dependência
    // é o container de DI configurado no Program.cs.
    // Vantagem: este serviço não precisa saber como o repositório é criado.
    public ServicoAutenticacao(IUsuarioRepository usuarioRepository, IPasswordHasher iPasswordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _iPasswordHasher = iPasswordHasher;
    }

    // Tenta autenticar o usuário com nome e senha fornecidos.
    // Retorna true se a autenticação for bem-sucedida, false caso contrário.
    //
    // "async Task<bool>" significa que este método é assíncrono (não bloqueia
    // a thread enquanto espera o banco responder) e retorna um booleano.
    public async Task<bool> LoginAsync(string username, string senha)
    {
        // Garante que sempre haja um usuário padrão para evitar um sistema
        // inacessível na primeira execução.
        // ⚠️ ATENÇÃO: o Program.cs já faz essa mesma verificação antes de
        // chegar aqui. A lógica está duplicada — inofensiva, mas redundante.
        await GarantirUsuarioPadraoAsync();

        // Busca o usuário pelo nome. Retorna null se não existir.
        var usuario = await _usuarioRepository.ObterPorUsernameAsync(username);

        // Se o usuário não foi encontrado, nega o acesso imediatamente.
        if (usuario == null) return false;

        // Delega a comparação de senha para o _iPasswordHasher.
        // Note que não acessamos "usuario.PasswordHash" fora do domínio de
        // segurança — passamos para quem sabe lidar com isso.
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

    // Método privado de "bootstrap": cria o usuário admin com senha "admin"
    // se o banco de dados estiver completamente vazio.
    // É privado porque é um detalhe interno deste serviço — ninguém de fora
    // precisa saber que essa garantia existe.
    private async Task GarantirUsuarioPadraoAsync()
    {
        if (!await _usuarioRepository.ExisteAlgumUsuarioAsync())
        {
            await CadastroUsuarioAsync(new CadastroUsuarioDto { Nome = "admin", Senha = "1234" });
        }
    }
}