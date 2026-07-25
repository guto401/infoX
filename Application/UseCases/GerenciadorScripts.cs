using Application.Interfaces;
using Domain.Interfaces;
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

    public async Task<string> ExecutarScriptFisicoAsync(
        string nomeArquivo,
        Action<string>? onLineRead = null,
        CancellationToken ct = default
    )
    {
        string caminhoCompleto = Path.Combine(_diretorioScripts, nomeArquivo);

        if (!File.Exists(caminhoCompleto))
            throw new FileNotFoundException($"O script '{nomeArquivo}' sumiu da pasta física!");

        string conteudoScript = await File.ReadAllTextAsync(caminhoCompleto);
        string resultado = string.Empty;
        StatusEnum status = StatusEnum.Concluido;

        try
        {
            ct.ThrowIfCancellationRequested();

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

            string comandoPowershell = await CSharpScript.EvaluateAsync<string>(
                conteudoScript,
                opcoes,
                cancellationToken: ct
            );

            if (string.IsNullOrWhiteSpace(comandoPowershell) || comandoPowershell == "VOLTAR")
            {
                return "[AVISO]: Operação cancelada no sub-menu.";
            }

            resultado = await _executor.ExecutarAsync(comandoPowershell, onLineRead, ct);

            if (resultado.Contains("[ERRO]") || resultado.Contains("[Exception]"))
            {
                status = StatusEnum.Erro;
            }
        }
        catch (OperationCanceledException)
        {
            status = StatusEnum.Cancelado;
            resultado += "\n[CANCELADO]: A execução foi interrompida pelo usuário(ESC).";
        }
        catch (Exception ex)
        {
            status = StatusEnum.Erro;
            resultado += $"[FALHA NA EXECUÇÃO]: {ex.Message}";
            return resultado;
        }
        finally
        {
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