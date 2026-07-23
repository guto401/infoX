// =============================================================================
// CAMADA: ConsoleApp (Ponto de entrada — a "casca" exterior da cebola)
// ARQUIVO: Program.cs
// =============================================================================
// O Program.cs tem TRÊS responsabilidades claras, e apenas essas:
//
//   1. BOOTSTRAP: Verificar pré-requisitos (admin, diretório de scripts)
//   2. COMPOSIÇÃO: Montar o container de Injeção de Dependência (DI)
//   3. ORQUESTRAÇÃO: Coordenar o fluxo principal (login → menu → execução)
//
// O Program.cs NÃO contém lógica de negócio — ele só conecta as peças.
// Toda inteligência está nos Use Cases (Application) e Serviços (Infrastructure).
//
// CONCEITO — Onion Architecture e o Program.cs:
// O Program.cs é a única camada que conhece TODAS as outras. Ele importa
// Application, Infrastructure e Domain. Isso é intencional — é a "cola"
// que monta o sistema, e existe exatamente um ponto de composição.
// =============================================================================

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
using Domain.Entities;
using Application.Security;

using System.Diagnostics;
using System.Security.Principal;

namespace ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        // =====================================================================
        // ETAPA 1: VERIFICAÇÃO DE PRIVILÉGIOS (UAC — User Account Control)
        // =====================================================================
        // Ferramentas de TI geralmente precisam de acesso administrativo para
        // alterar configurações do sistema, instalar software, modificar o registro, etc.
        //
        // WindowsPrincipal + IsInRole verifica se o processo atual tem o token
        // de administrador ativo (não só se o usuário É admin, mas se está
        // RODANDO com privilégios elevados).

        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        if (!isAdmin)
        {
            // Se não tem privilégios, relança o próprio executável pedindo elevação.
            // Verb = "runas" é o que aciona o prompt "Deseja permitir que este app
            // faça alterações no seu dispositivo?" do Windows.
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath, // Caminho do próprio .exe em execução
                UseShellExecute = true,             // OBRIGATÓRIO para o "runas" funcionar
                Verb = "runas"                      // Solicita elevação de privilégios
            };

            try
            {
                Process.Start(startInfo); // Abre a nova instância elevada
            }
            catch
            {
                // O usuário clicou "Não" no prompt do UAC — informamos e encerramos.
                Console.WriteLine("O usuário recusou a permissão de administrador.");
                Thread.Sleep(2000); // Pausa para o usuário ler a mensagem
            }

            return; // Encerra ESTA instância (sem privilégios) em qualquer caso
        }

        // =====================================================================
        // ETAPA 2: CONFIGURAÇÕES INICIAIS DO PROCESSO
        // =====================================================================

        // Lê a versão do assembly compilado (definida em ConsoleApp.csproj: <Version>1.0.0</Version>)
        // .ToString(3) formata como "Major.Minor.Patch" (ex: "1.0.0")
        var versaoInfoX = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

        // Força o terminal a usar UTF-8 para evitar problemas com caracteres
        // especiais (ã, ç, é, etc.) tanto na entrada quanto na saída.
        // Importante especialmente ao interagir com o output do PowerShell.
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        // =====================================================================
        // ETAPA 3: COMPOSIÇÃO DO CONTAINER DE INJEÇÃO DE DEPENDÊNCIA
        // =====================================================================
        // CONCEITO — Injeção de Dependência (DI):
        // Em vez de cada classe criar suas próprias dependências com "new X()",
        // declaramos o que cada interface deve resolver para uma implementação.
        // O container (ServiceCollection) cria e gerencia os objetos para nós.
        //
        // "AddScoped" significa que uma nova instância é criada por "escopo" —
        // aqui, efetivamente por operação, já que não usamos escopos explícitos.
        // Outras opções: AddSingleton (uma instância única), AddTransient (sempre nova).

        var services = new ServiceCollection();

        // Registra o DbContext do EF Core.
        // O EF Core sabe configurar o AppDbContext automaticamente.
        services.AddDbContext<AppDbContext>();

        // Ensina o container: "quando alguém pedir IUsuarioRepository, entregue SqliteRepository"
        // A Application só pede IUsuarioRepository — nunca sabe que é SqliteRepository.
        services.AddScoped<IUsuarioRepository, SqliteRepository>();
        services.AddScoped<IHistoricoRepository, SqliteRepository>();
        services.AddScoped<IExecutorBurro, ExecutorPowerShell>();

        // Use Cases são registrados diretamente (sem interface), pois o Program.cs
        // os referencia diretamente para orquestrar o fluxo.
        services.AddScoped<ServicoAutenticacao>();
        services.AddScoped<GerenciadorScripts>();

        // BuildServiceProvider "congela" as configurações e cria o container final.
        var serviceProvider = services.BuildServiceProvider();

        // =====================================================================
        // ETAPA 4: INTERFACE VISUAL — CABEÇALHO
        // =====================================================================
        // Spectre.Console é a biblioteca que fornece output rico no terminal:
        // texto colorido, figlets, tabelas, spinners, menus interativos, etc.

        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("InfoX")  // Arte ASCII grande com o nome do sistema
                .LeftJustified()
                .Color(Color.Blue));
        AnsiConsole.Write(new Rule($"[yellow]Sistema de Automação e Suporte - v{versaoInfoX}[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.Write(new Align(new Markup("[grey]by: @guto_marmiroli[/]"), HorizontalAlignment.Right));
        AnsiConsole.WriteLine();

        // =====================================================================
        // ETAPA 5: BOOTSTRAP DO USUÁRIO ADMIN
        // =====================================================================
        // Na primeira execução, o banco está vazio. Criamos o admin padrão
        // para que o sistema não fique inacessível.

        // GetRequiredService<T> recupera o serviço do container.
        // Se o serviço não estiver registrado, lança uma exceção clara.
        var usuarioRepo = serviceProvider.GetRequiredService<IUsuarioRepository>();
        bool temUsuario = await usuarioRepo.ExisteAlgumUsuarioAsync();

        if (!temUsuario)
        {
            // Object initializer — cria e inicializa as propriedades em uma expressão.
            // Note que este trecho USA O OBJECT INITIALIZER corretamente,
            // ao contrário do construtor parametrizado de Usuario (que tem o bug).
            var usuarioAdmin = new Usuario
            {
                Nome = "admin",
                PasswordHash = Argon2Helper.GerarHash("admin")
            };
            await usuarioRepo.CadastrarUsuarioAsync(usuarioAdmin);
            AnsiConsole.MarkupLine("[yellow]Primeira execução detectada: Usuário 'admin' criado com a senha 'admin'.[/]\n");
        }

        // =====================================================================
        // ETAPA 6: LOOP DE AUTENTICAÇÃO
        // =====================================================================
        // Mantém o prompt de login até que as credenciais sejam válidas.
        // Não há limite de tentativas — adicionar um lockout seria uma melhoria futura.

        var authService = serviceProvider.GetRequiredService<ServicoAutenticacao>();
        bool autenticado = false;

        while (!autenticado)
        {
            var username = AnsiConsole.Ask<string>("[cyan]Usuário:[/]");

            // TextPrompt com .Secret() exibe "*" em vez dos caracteres digitados.
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
        Thread.Sleep(1000); // Pequena pausa dramática antes de ir ao menu principal

        // =====================================================================
        // ETAPA 7: LOOP PRINCIPAL DO SISTEMA
        // =====================================================================
        // O "while (true)" cria um loop infinito. A única saída é via "break"
        // (usuário escolhe "Sair") ou via encerramento forçado do processo.
        //
        // A cada iteração: limpa a tela → lista scripts → exibe menu → executa escolha

        var gerenciador = serviceProvider.GetRequiredService<GerenciadorScripts>();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[blue]Scripts Disponíveis[/]").LeftJustified());

            // Lista os scripts .cs disponíveis na pasta Scripts/
            var scripts = gerenciador.ListarScriptsDisponiveis().ToList();

            // Se a pasta Scripts/ estiver vazia, informa e encerra.
            // "Any()" verifica se há ao menos um elemento na coleção.
            if (!scripts.Any())
            {
                AnsiConsole.MarkupLine("[red]Nenhum script .cs encontrado na pasta 'Scripts'.[/]");
                AnsiConsole.MarkupLine("Crie alguns arquivos .cs na pasta ao lado do executável.");
                break;
            }

            // Cria um item especial de saída e adiciona ao final da lista.
            // CaminhoCompleto vazio é o identificador de que é a opção "Sair".
            var opcaoSair = new ScriptLido { NomeArquivo = "Sair", CaminhoCompleto = "" };
            scripts.Add(opcaoSair);

            // SelectionPrompt cria um menu interativo navegável com setas do teclado.
            // UseConverter define como cada ScriptLido é exibido no menu.
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
                break; // Sai do while(true) — encerra o programa
            }

            // =====================================================================
            // EXECUÇÃO DO SCRIPT ESCOLHIDO
            // =====================================================================

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[green]Executando: {scriptEscolhido.NomeAmigavel}[/]").LeftJustified());
            AnsiConsole.WriteLine();

            // Define o callback que será chamado a cada linha do output do PowerShell.
            // Action<string> é um delegate — uma referência a uma função que aceita string.
            // Markup.Escape() escapa caracteres especiais do Spectre ("[", "]") para
            // que o output do PS não seja interpretado como markup de cor.
            Action<string> printarLinhaTempoReal = (linha) =>
            {
                AnsiConsole.MarkupLine($"[grey]>[/] {Markup.Escape(linha)}");
            };

            // AnsiConsole.Status() exibe um spinner animado enquanto a tarefa roda.
            // A task dentro do StartAsync é onde a execução real acontece.
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Aguardando processo...", async ctx =>
                {
                    ctx.Status("Processando script no Windows...");

                    // Aqui acontece o fluxo completo:
                    // 1. GerenciadorScripts lê o .cs e compila com Roslyn
                    // 2. O script C# exibe seu sub-menu e retorna o comando PS
                    // 3. ExecutorPowerShell executa o comando e chama o callback
                    // 4. O histórico é salvo no banco
                    await gerenciador.ExecutarScriptFisicoAsync(scriptEscolhido.NomeArquivo, printarLinhaTempoReal);
                });

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[green]Execução Finalizada[/]").LeftJustified());
            AnsiConsole.WriteLine();

            // Pausa antes de voltar ao menu para o usuário ter tempo de ler o output.
            AnsiConsole.MarkupLine("Pressione [blue]ENTER[/] para voltar ao menu...");
            Console.ReadLine();
        }
    }
}