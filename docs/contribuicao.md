# Contribuição e Roadmap

## Como Contribuir

### Pré-requisitos
- .NET SDK 10.0+
- Windows 10/11
- PowerShell disponível no PATH
- Privilégios de administrador (para testes)

### Configurando o Ambiente
1. Clone o repositório
2. Abra `Solution.slnx` no Visual Studio 2022+ ou execute via CLI
3. Restaure os pacotes: `dotnet restore`
4. Compile: `dotnet build`
5. Execute: `dotnet run --project ConsoleApp`

### Adicionando um Novo Script
A forma mais simples de contribuir é criando novos scripts de automação:

1. Crie um arquivo `.cs` na pasta `Scripts/`
2. Use o prefixo numérico para ordenar (ex: `10_MeuScript.cs`)
3. O script deve retornar uma string com comandos PowerShell
4. Use `Spectre.Console` para criar submenus interativos
5. Retorne `"VOLTAR"` para cancelar a operação
6. Não é necessário recompilar a aplicação

### Estrutura de um Script
```csharp
// Imports disponíveis: System, System.IO, Spectre.Console
// Variáveis disponíveis: AppContext.BaseDirectory

var opcao = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[bold]Meu Script[/]")
        .AddChoices("Opção 1", "Opção 2", "Voltar"));

if (opcao == "Voltar") return "VOLTAR";

// Gerar comandos PowerShell
var sb = new System.Text.StringBuilder();
sb.AppendLine("Write-Host 'Executando...' -ForegroundColor Green");
sb.AppendLine("Get-Process | Select-Object -First 5");
return sb.ToString();
```

### Publicando
Comandos de publish disponíveis:
```bash
# Single file self-contained
dotnet publish ConsoleApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Com native libraries extraídas
dotnet publish ConsoleApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Multi-file self-contained
dotnet publish ConsoleApp -c Release -r win-x64 --self-contained true
```

## Roadmap

### Implementado
- [x] Download de ferramentas antivírus
- [x] Manutenção do sistema (CHKDSK, SFC, DISM)
- [x] Gerenciamento de pacotes (Winget)
- [x] Limpezas profundas (11 categorias)
- [x] Inventário de hardware/software
- [x] Backup pré-formatação (Chrome, Outlook)
- [x] Definição de programas padrão
- [x] Otimização do Agendador de Tarefas
- [x] Configuração de Firewall (hardening + rede local)
- [x] Gerenciamento de Pontos de Restauração
- [x] Debloat do Windows 11
- [x] Autenticação com Argon2id
- [x] Histórico de execuções (SQLite)

### Pendente
- [ ] Abrir CMD
- [ ] Corrigir data e hora
- [ ] Otimização do computador
- [ ] Setup inicial do PC
- [ ] Setup do painel de energia
- [ ] Gerenciamento de apps no startup

### Melhorias Planejadas na Interface
- [ ] Rodapé com última execução registrada
- [ ] Recarregar scripts sem reiniciar
- [ ] Visualizador interativo do histórico
- [ ] Módulo de gerenciamento de usuários (CRUD)

## CI/CD
O diretório `.github/workflows/` existe no repositório mas atualmente está **vazio** — não há pipelines de CI/CD configurados.
