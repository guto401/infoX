using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Application.UseCases;
using Application.Interfaces;
using Infrastructure.Context;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Application.Models;

namespace ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Configuração da Injeção de Dependência (O "Coração" da Onion)
        var services = new ServiceCollection();

        // Registrando o Banco de Dados
        services.AddDbContext<AppDbContext>();

        // Explicando para o C#: "Quando alguém pedir uma Interface X, entregue a Classe Y"
        services.AddScoped<IUsuarioRepository, SqliteRepository>();
        services.AddScoped<IHistoricoRepository, SqliteRepository>();
        services.AddScoped<IExecutorBurro, ExecutorPowerShell>();

        // Registrando os nossos Casos de Uso
        services.AddScoped<ServicoAutenticacao>();
        services.AddScoped<GerenciadorScripts>();

        var serviceProvider = services.BuildServiceProvider();

        // 2. Interface Visual - Cabeçalho InfoX
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("InfoX Admin")
                .LeftJustified()
                .Color(Color.Blue));
        AnsiConsole.Write(new Rule("[yellow]Sistema de Automação e Suporte[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();

        // 3. Orquestrando o Login
        var authService = serviceProvider.GetRequiredService<ServicoAutenticacao>();
        bool autenticado = false;

        while (!autenticado)
        {
            var username = AnsiConsole.Ask<string>("[cyan]Usuário:[/]");
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("[cyan]Senha:[/]")
                    .PromptStyle("red")
                    .Secret()); // Esconde a digitação

            autenticado = await authService.LoginAsync(username, password);

            if (!autenticado)
            {
                AnsiConsole.MarkupLine("[red]Credenciais inválidas! Tente novamente.[/]\n");
            }
        }

        AnsiConsole.MarkupLine("[green]Autenticação bem-sucedida![/]\n");
        Thread.Sleep(1000); // Pausa dramática rápida

        // 4. Loop Principal do Sistema
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

            // Adiciona uma opção de saída na lista
            var opcaoSair = new ScriptLido { NomeArquivo = "Sair", CaminhoCompleto = "" };
            scripts.Add(opcaoSair);

            // O Menu Interativo
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

            // 5. Execução em Tempo Real
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[green]Executando: {scriptEscolhido.NomeAmigavel}[/]").LeftJustified());
            AnsiConsole.WriteLine();

            // Usamos o Action (callback) que criamos para printar as linhas assim que o PowerShell cospe
            Action<string> printarLinhaTempoReal = (linha) =>
            {
                AnsiConsole.MarkupLine($"[grey]>[/] {Markup.Escape(linha)}");
            };

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Aguardando processo...", async ctx =>
                {
                    ctx.Status("Processando script no Windows...");

                    // Chama a aplicação, que chama a infraestrutura, que chama o PowerShell
                    await gerenciador.ExecutarScriptFisicoAsync(scriptEscolhido.NomeArquivo, printarLinhaTempoReal);
                });

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[green]Execução Finalizada[/]").LeftJustified());
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("Pressione [blue]ENTER[/] para voltar ao menu...");
            Console.ReadLine();
        }
    }
}