using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Application.UseCases;
using Application.Interfaces;
using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Application.Models;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;
using System.Security.Principal;

namespace ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        if (!isAdmin)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
            }
            catch
            {
                Console.WriteLine("O usuário recusou a permissão de administrador.");
                Thread.Sleep(2000);
            }

            return;
        }

        var versaoInfoX = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();

        services.AddScoped<IUsuarioRepository, SqliteRepository>();
        services.AddScoped<IHistoricoRepository, SqliteRepository>();
        services.AddScoped<IExecutorBurro, ExecutorPowerShell>();
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();

        services.AddScoped<ServicoAutenticacao>();
        services.AddScoped<GerenciadorScripts>();

        var serviceProvider = services.BuildServiceProvider();

        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("InfoX")
                .LeftJustified()
                .Color(Color.Blue));
        AnsiConsole.Write(new Rule($"[yellow]Sistema de Automação e Suporte - v{versaoInfoX}[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.Write(new Align(new Markup("[grey]by: @guto_marmiroli[/]"), HorizontalAlignment.Right));
        AnsiConsole.WriteLine();

        var authService = serviceProvider.GetRequiredService<ServicoAutenticacao>();
        bool autenticado = false;

        while (!autenticado)
        {
            var username = AnsiConsole.Ask<string>("[cyan]Usuário:[/]");

            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Senha:[/]")
                    .PromptStyle("red")
                    .Secret());

            autenticado = await authService.LoginAsync(username, password);

            if (!autenticado)
            {
                AnsiConsole.MarkupLine("[red]Credenciais inválidas! Tente novamente.[/]\n");
            }
        }

        AnsiConsole.MarkupLine("[green]Autenticação bem-sucedida![/]\n");
        Thread.Sleep(1000);

        var gerenciador = serviceProvider.GetRequiredService<GerenciadorScripts>();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[blue]Scripts Disponíveis[/]").LeftJustified());

            var scripts = gerenciador.ListarScriptsDisponiveis().ToList();

            if (!scripts.Any())
            {
                AnsiConsole.MarkupLine("[red]Nenhum script .cs encontrado na pasta 'Scripts'.[/]");
                AnsiConsole.MarkupLine("Crie alguns arquivos .cs na pasta ao lado do executável.");
                break;
            }

            var opcaoSair = new ScriptLido { NomeArquivo = "Sair", CaminhoCompleto = "" };
            scripts.Add(opcaoSair);

            var scriptEscolhido = AnsiConsole.Prompt(
                new SelectionPrompt<ScriptLido>()
                    .Title("Selecione o script para execução (Use as setas):")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Mova para cima/baixo para ver mais)[/]")
                    .UseConverter(s => s.NomeArquivo == "Sair" ? "[red]Sair do Sistema[/]" : s.NomeAmigavel)
                    .AddChoices(scripts)
            );

            if (scriptEscolhido.NomeArquivo == "Sair")
            {
                AnsiConsole.MarkupLine("[yellow]Encerrando o InfoX...[/]");
                break;
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[green]Executando: {scriptEscolhido.NomeAmigavel}[/]").LeftJustified());
            AnsiConsole.WriteLine();

            Action<string> printarLinhaTempoReal = (linha) =>
            {
                AnsiConsole.MarkupLine($"[grey]>[/] {Markup.Escape(linha)}");
            };

            using var cts = new CancellationTokenSource();
            using var keyMonitorCts = new CancellationTokenSource();

            var monitorTecla = Task.Run(() =>
            {
                while (!keyMonitorCts.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var tecla = Console.ReadKey(intercept: true);
                        if (tecla.Key == ConsoleKey.Escape)
                        {
                            cts.Cancel();
                            break;
                        }
                    }
                    Thread.Sleep(50);
                }
            }, keyMonitorCts.Token);

            ConsoleCancelEventHandler cancelHandler = (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
            Console.CancelKeyPress += cancelHandler;

            string resultadoExecucao = string.Empty;

            try
            {
                resultadoExecucao = await gerenciador.ExecutarScriptFisicoAsync(
                    scriptEscolhido.NomeArquivo,
                    printarLinhaTempoReal,
                    cts.Token);

                if (!string.IsNullOrWhiteSpace(resultadoExecucao))
                {
                    AnsiConsole.WriteLine();
                    if (resultadoExecucao.Contains("[FALHA NA EXECUÇÃO]"))
                    {
                        AnsiConsole.Write(new Rule("[red]Erro na Execução do Script[/]").LeftJustified());
                        AnsiConsole.MarkupLine($"[red]{Markup.Escape(resultadoExecucao)}[/]");
                    }
                    else if (resultadoExecucao.StartsWith("[AVISO]") || resultadoExecucao.Contains("[CANCELADO]"))
                    {
                        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(resultadoExecucao)}[/]");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("\n[yellow]Execução cancelada pelo usuário.[/]");
            }
            finally
            {
                keyMonitorCts.Cancel();
                Console.CancelKeyPress -= cancelHandler;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[green]Execução Finalizada[/]").LeftJustified());
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("Pressione [blue]ENTER[/] para voltar ao menu...");
            Console.ReadLine();
        }
    }
}