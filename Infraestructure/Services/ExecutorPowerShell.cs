using Application.Interfaces;
using System.Diagnostics;
using System.Text;

namespace Infrastructure.Services;

public class ExecutorPowerShell : IExecutorBurro
{
    public async Task<string> ExecutarAsync(string scriptConteudo, Action<string>? onLineRead = null)
    {
        if (string.IsNullOrWhiteSpace(scriptConteudo))
            return "Script vazio.";

        var processInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            // -NoProfile pula o carregamento de configurações do usuário (ganho bruto de velocidade)
            // -Command diz ao PS para executar a instrução literal que enviamos
            Arguments = $"-NoProfile -NonInteractive -Command \"{scriptConteudo.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = processInfo };
        var outputCompleto = new StringBuilder();

        // Escuta e repassa o Output padrão linha por linha
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputCompleto.AppendLine(e.Data);
                onLineRead?.Invoke(e.Data); // Callback em tempo real
            }
        };

        // Escuta e repassa possíveis erros de execução
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputCompleto.AppendLine($"[ERRO]: {e.Data}");
                onLineRead?.Invoke($"[red][ERRO]: {e.Data}[/]"); // Formatação de cor Spectre
            }
        };

        try
        {
            process.Start();

            // Ativa a leitura assíncrona das streams
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return outputCompleto.ToString();
        }
        catch (Exception ex)
        {
            return $"[FALHA INTERNA DO PROCESSO]: {ex.Message}";
        }
    }
}