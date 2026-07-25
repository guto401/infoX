using Application.Interfaces;
using System.Diagnostics;
using System.Text;

namespace Infrastructure.Services;

public class ExecutorPowerShell : IExecutorBurro
{
    public async Task<string> ExecutarAsync(
        string scriptConteudo, 
        Action<string>? onLineRead = null,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(scriptConteudo))
            return "Script vazio.";

        ct.ThrowIfCancellationRequested();
        
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

        using var registration = ct.Register(() =>  {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch{ }
        });

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(ct);

            return outputCompleto.ToString();
        }
        catch (OperationCanceledException)
        {
            outputCompleto.AppendLine("\n[ALERTA]: Execução do processo PowerShell cancelada pelo usuário.");
            throw;
        }
        catch (Exception ex)
        {
            return $"[FALHA INTERNA DO PROCESSO]: {ex.Message}";
        }
    }
}