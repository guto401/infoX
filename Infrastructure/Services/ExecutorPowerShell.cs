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
            Arguments = $"-NoProfile -NonInteractive -Command \"[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; {scriptConteudo.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = processInfo };
        var outputCompleto = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputCompleto.AppendLine(e.Data);
                onLineRead?.Invoke(e.Data);
            }
        };

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
            process.Start();
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