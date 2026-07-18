using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Spectre.Console;

namespace Application.UseCases;

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

    public IEnumerable<ScriptLido> ListarScriptsDisponiveis()
    {
        var arquivos = Directory.GetFiles(_diretorioScripts, "*.cs");

        return arquivos.Select(caminho => new ScriptLido
        {
            NomeArquivo = Path.GetFileName(caminho),
            CaminhoCompleto = caminho
        });
    }

    public async Task<string> ExecutarScriptFisicoAsync(string nomeArquivo, Action<string>? onLineRead = null)
    {
        string caminhoCompleto = Path.Combine(_diretorioScripts, nomeArquivo);

        if (!File.Exists(caminhoCompleto))
            throw new FileNotFoundException($"O script '{nomeArquivo}' sumiu da pasta física!");

        // Lê o código do .cs
        string conteudoScript = await File.ReadAllTextAsync(caminhoCompleto);
        string resultado = string.Empty;
        StatusEnum status = StatusEnum.Concluido;

        try
        {
            // Ensinamos o Roslyn a reconhecer dlls e dependências
            var opcoes = ScriptOptions.Default
                .AddReferences(
                    typeof(System.IO.Path).Assembly,
                    typeof(AppContext).Assembly,
                    typeof(AnsiConsole).Assembly
                )
                .AddImports(
                    "System",
                    "System.IO",
                    "Spectre.Console"
                );

            // O Roslyn executa o C# na memória e captura estritamente o valor do 'return
            string comandoPowershell = await CSharpScript.EvaluateAsync<string>(conteudoScript, opcoes);

            // Se o usuário escolher cancelar dentro do sub-menu do script, tratamos aqui
            if (string.IsNullOrWhiteSpace(comandoPowershell) || comandoPowershell == "VOLTAR")
            {
                return "[AVISO]: Operação cancelada no sub-menu.";
            }

            // Passa a string retornada para o executor burro rodar no PowerShell
            resultado = await _executor.ExecutarAsync(comandoPowershell, onLineRead);

            if (resultado.Contains("[ERRO]") || resultado.Contains("[Exception]"))
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
            // Independente de dar certo ou errado, salvamos no SQLite (Auditoria)
            var historico = new HistoricoExecucao
            {
                NomeScript = nomeArquivo,
                DataExecucao = DateTime.Now,
                Status = status,
                OutputLog = resultado
            };

            await _historicoRepository.SalvarAsync(historico);
        }

        return resultado;
    }
}