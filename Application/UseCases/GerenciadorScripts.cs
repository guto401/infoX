using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace MeuProjeto.Application.UseCases;

public class GerenciadorScripts
{
    private readonly IExecutorBurro _executor;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly string _diretorioScripts;

    public GerenciadorScripts(IExecutorBurro executor, IHistoricoRepository historicoRepository)
    {
        _executor = executor;
        _historicoRepository = historicoRepository;

        // Define que os scripts ficam em uma pasta "Scripts" no mesmo local do executável do seu app
        _diretorioScripts = Path.Combine(AppContext.BaseDirectory, "Scripts");

        if (!Directory.Exists(_diretorioScripts))
        {
            Directory.CreateDirectory(_diretorioScripts);
        }
    }

    // Lê a pasta em tempo real (Sem latência, sem precisar sincronizar banco)
    public IEnumerable<string> ListarScriptsDisponiveis()
    {
        return Directory.GetFiles(_diretorioScripts, "*.cs")
                        .Select(Path.GetFileName)
                        .Where(name => name != null)!;
    }

    public async Task<string> ExecutarScriptFisicoAsync(string nomeArquivo, Action<string>? onLineRead = null)
    {
        string caminhoCompleto = Path.Combine(_diretorioScripts, nomeArquivo);

        if (!File.Exists(caminhoCompleto))
            throw new FileNotFoundException($"O script '{nomeArquivo}' sumiu da pasta física!");

        // Lê o código .cs que chama o Process do cmd.exe que você já tem pronto
        string conteudoScript = await File.ReadAllTextAsync(caminhoCompleto);

        string resultado = string.Empty;
        StatusEnum status = StatusEnum.Concluido;

        try
        {
            // Passa para o executor burro rodar no PowerShell
            resultado = await _executor.ExecutarAsync(conteudoScript, onLineRead);

            if (resultado.Contains("[Erro no CMD]") || resultado.Contains("[ERRO]"))
            {
                status = StatusEnum.Erro;
            }
        }
        catch (Exception ex)
        {
            status = StatusEnum.Erro;
            resultado = $"[FALHA NA EXECUÇÃO]: {ex.Message}";
        }
        finally
        {
            // Salva o histórico no SQLite para auditoria posterior
            var historico = new HistoricoExecucao(nomeArquivo, status, resultado);
            await _historicoRepository.SalvarAsync(historico);
        }

        return resultado;
    }
}