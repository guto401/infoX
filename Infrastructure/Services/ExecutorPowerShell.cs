// =============================================================================
// CAMADA: Infrastructure (Detalhes técnicos — depende de Application e Domain)
// ARQUIVO: ExecutorPowerShell.cs
// =============================================================================
// Este é o "músculo" do InfoX — a implementação concreta do IExecutorBurro.
//
// CONCEITO — Por que abrir o powershell.exe como processo filho?
// O .NET permite executar comandos shell via System.Diagnostics.Process.
// Abrimos um processo filho do powershell.exe, passamos o comando via argumento,
// capturamos o output (stdout e stderr), e retornamos tudo como string.
//
// Vantagens desta abordagem:
// - O PowerShell tem acesso nativo ao Windows: WMI, Registro, AD, etc.
// - Scripts PS existentes rodam sem modificação
// - A saída é capturada de forma assíncrona (não bloqueia a thread .NET)
//
// CONCEITO — Por que "Burro"?
// Este executor não interpreta, não valida, não pensa. Ele recebe uma string
// e abre o PowerShell com ela. A inteligência fica no GerenciadorScripts
// (que usa Roslyn para executar a lógica C# e montar o comando).
// =============================================================================

using Application.Interfaces;
using System.Diagnostics;
using System.Text;

namespace Infrastructure.Services;

public class ExecutorPowerShell : IExecutorBurro
{
    // Implementação do contrato definido em IExecutorBurro.
    // Recebe uma string de comandos PowerShell e retorna o output completo.
    public async Task<string> ExecutarAsync(string scriptConteudo, Action<string>? onLineRead = null)
    {
        // Guarda de entrada: rejeita execuções de string vazia imediatamente.
        if (string.IsNullOrWhiteSpace(scriptConteudo))
            return "Script vazio.";

        // ProcessStartInfo configura como o processo filho será iniciado.
        var processInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",

            // Flags importantes passadas ao PowerShell:
            // -NoProfile     → Não carrega o perfil do usuário (~\Documents\WindowsPowerShell\profile.ps1)
            //                  Ganho de velocidade significativo na inicialização.
            // -NonInteractive → Impede que o PS fique esperando input do usuário
            // -Command        → Executa a string literal que passamos como argumento
            //
            // O bloco "[Console]::OutputEncoding = ..." força o PS a usar UTF-8
            // ANTES de executar nosso comando, prevenindo bugs de acentuação
            // (sem isso, caracteres como ã, ç, é aparecem como lixo no output).
            //
            // Replace('"', '\"') escapa aspas duplas dentro do comando para que
            // o shell não confunda com o fim da string de argumento.
            Arguments = $"-NoProfile -NonInteractive -Command \"[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; {scriptConteudo.Replace("\"", "\\\"")}\"",

            RedirectStandardOutput = true, // Captura o stdout (saída normal) do PS
            RedirectStandardError = true,  // Captura o stderr (erros) do PS
            UseShellExecute = false,       // OBRIGATÓRIO para redirecionar streams
            CreateNoWindow = true,         // Não abre uma janela visível do PowerShell

            // Garante que as streams sejam decodificadas como UTF-8 corretamente.
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // "using var" garante que o Process seja descartado (Dispose) ao sair do escopo,
        // liberando os handles do processo e evitando vazamento de recursos.
        using var process = new Process { StartInfo = processInfo };
        var outputCompleto = new StringBuilder();

        // CONCEITO — Eventos de output assíncrono:
        // Em vez de esperar o processo terminar e ler tudo de uma vez,
        // registramos handlers que são chamados a cada linha produzida.
        // Isso permite o streaming em tempo real para a UI.

        // Handler para output normal (stdout)
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputCompleto.AppendLine(e.Data);  // Acumula para retornar no final
                onLineRead?.Invoke(e.Data);          // Notifica a UI em tempo real
                                                     // O "?." é o operador null-conditional:
                                                     // só chama Invoke se onLineRead não for null
            }
        };

        // Handler para erros (stderr) — prefixamos com [ERRO] para identificação
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputCompleto.AppendLine($"[ERRO]: {e.Data}");
                onLineRead?.Invoke($"[ERRO]: {e.Data}");
            }
        };

        try
        {
            process.Start(); // Inicia o processo filho

            // Ativa a leitura assíncrona das streams APÓS o processo iniciar.
            // Sem isso, os eventos OutputDataReceived/ErrorDataReceived nunca disparam.
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Aguarda o processo terminar de forma assíncrona (sem bloquear a thread).
            // WaitForExitAsync é o equivalente async de WaitForExit().
            await process.WaitForExitAsync();

            return outputCompleto.ToString();
        }
        catch (Exception ex)
        {
            // Captura erros no LANÇAMENTO do processo (ex: powershell.exe não encontrado).
            // Erros DE EXECUÇÃO do script PS são capturados pelo stderr acima.
            return $"[FALHA INTERNA DO PROCESSO]: {ex.Message}";
        }
    }
}