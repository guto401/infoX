# InfoX — Visão Geral

## O que é o InfoX?

O **InfoX** é um utilitário avançado para **administração de sistemas Windows**, diagnósticos e automação de suporte técnico em TI. É uma aplicação console interativa construída em **.NET 10** que utiliza uma arquitetura híbrida onde **C# (Roslyn) é o "cérebro"** e **PowerShell é o "músculo"**.

Scripts C# (`.cs`) são compilados dinamicamente em memória pelo Roslyn e geram comandos PowerShell que são executados pelo sistema, permitindo criar automações complexas com menus interativos — **sem necessidade de recompilar a aplicação**.

## Versão Atual

**v1.4.1**

## Stack Tecnológica

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| .NET | 10.0 | Framework principal |
| Entity Framework Core | 10.0.10 | ORM para SQLite |
| SQLite | 3.53.3 | Banco de dados embarcado |
| Microsoft.CodeAnalysis (Roslyn) | 5.6.0 | Compilação dinâmica de scripts C# |
| Spectre.Console | 0.57.2 | Interface de terminal rica |
| Konscious.Security.Cryptography | 1.3.1 | Hashing Argon2id |
| PowerShell | — | Execução de comandos do SO |

## Funcionalidades Principais

### 🛡️ Segurança e Antivírus
1. **Download de ferramentas antivírus** — Kaspersky KVRT, Malwarebytes AdwCleaner, RogueKiller

### 🔧 Manutenção do Sistema
2. **Diagnóstico e reparo** — CHKDSK, SFC, DISM (inteligente com Scan + Restore condicional)
3. **Gerenciamento de pacotes** — Winget (instalação, atualização, CCleaner Portable)
4. **Limpeza profunda** — 11 categorias de limpeza (temporários, logs, caches, lixeira, dumps, etc.)

### 📊 Inventário e Backup
5. **Inventário completo** — Hardware, software, rede, impressoras, usuários via WMI/CIM
6. **Backup pré-formatação** — Chrome, Outlook, relatório completo em TXT

### ⚙️ Configuração do Sistema
7. **Programas padrão** — Associações de arquivos (Chrome, Adobe, 7-Zip)
8. **Agendador de Tarefas** — Desativação de telemetria, CEIP, diagnósticos
9. **Firewall** — Hardening de segurança, rede local, auditoria com log
10. **Pontos de Restauração** — Gerenciamento completo de VSS

### 🧹 Otimização
11. **Debloat do Windows 11** — Remoção de bloatware

## Características Técnicas

- ✅ **Elevação automática de privilégios (UAC)** — requer administrador
- ✅ **Autenticação com Argon2id** — hashing seguro com proteção contra timing attacks
- ✅ **Cancelamento seguro** — Ctrl+C e ESC com `Kill(entireProcessTree: true)`
- ✅ **Streaming em tempo real** — output do PowerShell renderizado linha a linha
- ✅ **Persistência de histórico** — cada execução registrada no SQLite
- ✅ **Interface rica** — Spectre.Console com menus, pesquisa, Figlet, cores
- ✅ **Encoding UTF-8** — `chcp 65001` forçado para compatibilidade PT-BR
- ✅ **Scripts extensíveis** — basta adicionar `.cs` na pasta Scripts/

## Requisitos

- Windows 10/11
- Privilégios de Administrador
- PowerShell disponível no PATH

## Como Buildar

```bash
# Build padrão com single file
dotnet publish ConsoleApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Build com native libraries incluídas
dotnet publish ConsoleApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Build sem single file (multi-file)
dotnet publish ConsoleApp -c Release -r win-x64 --self-contained true
```

## Estrutura do Repositório

```
infoX/
├── Solution.slnx            # Solução .NET (formato XML moderno)
├── README.md                 # README do repositório
├── Domain/                   # Camada de Domínio (entidades, enums, interfaces)
├── Application/              # Camada de Aplicação (use cases, DTOs, interfaces)
├── Infrastructure/           # Camada de Infraestrutura (EF Core, PowerShell, Argon2)
├── ConsoleApp/               # Camada de Apresentação (Program.cs, UI terminal)
├── Scripts/                  # Scripts C# dinâmicos (compilados pelo Roslyn)
└── docs/                     # Documentação do projeto
```

## Documentação

| Documento | Descrição |
|-----------|-----------|
| [arquitetura.md](arquitetura.md) | Arquitetura do projeto (Onion Architecture, DI, fluxo de dados) |
| [dominio.md](dominio.md) | Camada de Domínio (entidades, enums, interfaces) |
| [infraestrutura.md](infraestrutura.md) | Camada de Infraestrutura (EF Core, Argon2, PowerShell) |
| [console-app.md](console-app.md) | Camada de Apresentação (Program.cs, Spectre.Console) |
| [database.md](database.md) | Banco de dados SQLite (schema, repositório, seed) |
| [scripts.md](scripts.md) | Catálogo e guia de scripts de automação |
| [seguranca.md](seguranca.md) | Segurança (Argon2id, UAC, firewall, cancelamento) |
| [contribuicao.md](contribuicao.md) | Contribuição e roadmap do projeto |
