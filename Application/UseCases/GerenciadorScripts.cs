// =============================================================================
// CAMADA: Application (Regras de negócio — depende apenas do Domain)
// ARQUIVO: GerenciadorScripts.cs
// =============================================================================
// Este é o "cérebro" do InfoX — o Use Case central que orquestra o fluxo
// completo de execução de um script. O fluxo tem 4 etapas:
//
//  1. LÊ o arquivo .cs físico do disco
//  2. COMPILA e EXECUTA o C# em memória via Roslyn (CSharpScript)
//     O script C# exibe seus menus e retorna uma string de comando PowerShell
//  3. PASSA a string de comando para o IExecutorBurro (PowerShell)
//  4. SALVA o resultado no banco de dados (auditoria/histórico)
//
// CONCEITO — Roslyn Scripting:
// O Roslyn é o compilador do C# exposto como uma biblioteca. Com ele, podemos
// compilar e executar código C# em tempo real, em memória, sem precisar criar
// um .exe separado. É como ter um interpretador C# embutido no programa.
//
// CSharpScript.EvaluateAsync<string>() compila o código, roda, e retorna
// o valor da última expressão do script (que deve ser uma string de comandos PS).
// =============================================================================

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
    // O "músculo": executor que roda os comandos no PowerShell.
    // Recebido como IExecutorBurro (interface), não como ExecutorPowerShell (classe).
    private readonly IExecutorBurro _executor;

    // Repositório de auditoria: onde os registros de execução são salvos.
    private readonly IHistoricoRepository _historicoRepository;

    // Caminho absoluto para a pasta Scripts/, calculado uma única vez no construtor.
    private readonly string _diretorioScripts;

    // Construtor com injeção de dependências.
    // O container de DI resolve automaticamente as implementações concretas.
    public GerenciadorScripts(IExecutorBurro executor, IHistoricoRepository historicoRepository)
    {
        _executor = executor;
        _historicoRepository = historicoRepository;

        // AppContext.BaseDirectory retorna o diretório onde o .exe está localizado.
        // É mais confiável que Environment.CurrentDirectory, que pode variar
        // dependendo de onde o usuário iniciou o programa no terminal.
        _diretorioScripts = Path.Combine(AppContext.BaseDirectory, "Scripts");

        // Resiliência na inicialização: se a pasta não existe, cria automaticamente.
        // Isso evita FileNotFoundException logo na primeira execução.
        if (!Directory.Exists(_diretorioScripts))
        {
            Directory.CreateDirectory(_diretorioScripts);
        }
    }

    // Varre a pasta Scripts/ e retorna todos os arquivos .cs encontrados.
    // Retorna um IEnumerable<ScriptLido> — uma sequência lazy de objetos ScriptLido.
    // Cada ScriptLido contém o nome e o caminho completo do arquivo.
    public IEnumerable<ScriptLido> ListarScriptsDisponiveis()
    {
        var arquivos = Directory.GetFiles(_diretorioScripts, "*.cs");

        // "Select" é um método LINQ que transforma cada elemento de uma coleção
        // em outro tipo. Aqui transformamos cada string (caminho) em um ScriptLido.
        // A expressão "=>" é uma "lambda" — uma função anônima definida na hora.
        return arquivos.Select(caminho => new ScriptLido
        {
            NomeArquivo = Path.GetFileName(caminho),   // "LimpezaTemp.cs"
            CaminhoCompleto = caminho                   // "C:\InfoX\Scripts\LimpezaTemp.cs"
        });
    }

    // Executa um script .cs pelo nome do arquivo.
    // Este é o método mais complexo e importante do projeto.
    //
    // Parâmetros:
    //   nomeArquivo — nome do arquivo .cs (ex: "LimpezaTemp.cs")
    //   onLineRead  — callback chamado a cada linha de output do PowerShell em tempo real
    //
    // Retorna o output completo da execução como string.
    public async Task<string> ExecutarScriptFisicoAsync(string nomeArquivo, Action<string>? onLineRead = null)
    {
        string caminhoCompleto = Path.Combine(_diretorioScripts, nomeArquivo);

        // Verificação defensiva: e se o arquivo foi deletado entre o menu e a execução?
        if (!File.Exists(caminhoCompleto))
            throw new FileNotFoundException($"O script '{nomeArquivo}' sumiu da pasta física!");

        // Lê o conteúdo do arquivo .cs como texto puro.
        // ReadAllTextAsync é assíncrono — não bloqueia a thread durante a leitura.
        string conteudoScript = await File.ReadAllTextAsync(caminhoCompleto);
        string resultado = string.Empty;
        StatusEnum status = StatusEnum.Concluido; // Assumimos sucesso e corrigimos se necessário

        try
        {
            // =========================================================
            // ETAPA 2: COMPILAR E EXECUTAR O SCRIPT C# VIA ROSLYN
            // =========================================================

            // ScriptOptions configura o ambiente de compilação do script.
            // Sem isso, o Roslyn não saberia resolver tipos como "AnsiConsole".
            var opcoes = ScriptOptions.Default
                .AddReferences(
                    // Adicionamos referências às DLLs que o script pode usar.
                    // typeof(X).Assembly é a forma idiomática de obter a DLL
                    // que contém o tipo X, sem hardcodar caminhos de arquivo.
                    typeof(System.IO.Path).Assembly,   // System.dll (tipos básicos)
                    typeof(AppContext).Assembly,        // System.Runtime.dll
                    typeof(AnsiConsole).Assembly        // Spectre.Console.dll
                )
                .AddImports(
                    // Equivalente a adicionar "using X;" no topo do script.
                    // Sem isso, o script teria que escrever "System.Console.WriteLine"
                    // em vez de apenas "Console.WriteLine".
                    "System",
                    "System.IO",
                    "Spectre.Console"
                );

            // O momento mágico: o Roslyn compila o conteúdo do arquivo .cs
            // na memória e o executa como se fosse código C# normal.
            //
            // EvaluateAsync<string> significa que esperamos que a última
            // expressão do script seja uma string — o comando PowerShell a executar.
            // O script pode mostrar menus, fazer perguntas ao usuário, e no final
            // retornar a string de comando construída com as escolhas do usuário.
            string comandoPowershell = await CSharpScript.EvaluateAsync<string>(conteudoScript, opcoes);

            // Se o script retornou vazio ou a string especial "VOLTAR",
            // significa que o usuário cancelou a operação no sub-menu.
            // Retornamos sem executar nada no PowerShell.
            if (string.IsNullOrWhiteSpace(comandoPowershell) || comandoPowershell == "VOLTAR")
            {
                return "[AVISO]: Operação cancelada no sub-menu.";
            }

            // =========================================================
            // ETAPA 3: EXECUTAR O COMANDO NO POWERSHELL
            // =========================================================

            // Passa a string de comandos para o ExecutorPowerShell (via IExecutorBurro).
            // O callback onLineRead permite que a UI mostre cada linha assim que chega,
            // em vez de esperar toda a execução terminar para mostrar de uma vez.
            resultado = await _executor.ExecutarAsync(comandoPowershell, onLineRead);

            // Detecta falhas pelo conteúdo do output (heurística simples).
            // Uma abordagem mais robusta seria usar o exit code do processo.
            if (resultado.Contains("[ERRO]") || resultado.Contains("[Exception]"))
            {
                status = StatusEnum.Erro;
            }
        }
        catch (Exception ex)
        {
            // Captura erros tanto do Roslyn (script C# inválido) quanto
            // de qualquer outra exceção inesperada.
            status = StatusEnum.Erro;
            resultado = $"[FALHA NA EXECUÇÃO]: {ex.Message}";
        }
        finally
        {
            // =========================================================
            // ETAPA 4: SALVAR O HISTÓRICO (AUDITORIA)
            // =========================================================

            // CONCEITO — bloco "finally":
            // O código dentro de "finally" é SEMPRE executado, independente
            // de ter ocorrido uma exceção no try/catch ou não.
            // É o lugar ideal para operações de limpeza e auditoria.
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