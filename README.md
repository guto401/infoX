# 🛠️ InfoX — Sistema de Automação e Suporte Tático

![GitHub Repo stars](https://img.shields.io/github/stars/guto_marmiroli/infoX?style=for-the-badge)
![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![Architecture](https://img.shields.io/badge/Architecture-Onion-ff69b4?style=for-the-badge)
![OS Target](https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

O **InfoX** é um utilitário interativo de console focado em automação, suporte tático e gerenciamento de infraestrutura de TI. Desenvolvido como **Trabalho de Conclusão de Curso (TCC)**, o projeto utiliza conceitos avançados de engenharia de software para oferecer um console de alto desempenho para técnicos de campo e administradores de sistemas.

---

## 🧠 Arquitetura e Engenharia do Sistema

O InfoX adota a **Onion Architecture** (Arquitetura Cebola) para garantir total desacoplamento entre as regras de negócio e a infraestrutura, facilitando a portabilidade do motor de automação para futuros agentes (como serviços RMM em background).

### O Segredo: Orquestração Híbrida
O sistema resolve o problema clássico de balancear flexibilidade lógica com controle de sistema operacional dividindo a execução em duas frentes:

1. **O Cérebro (C# + Roslyn):** A aplicação lê scripts físicos `.cs` dinamicamente de um diretório local. Utilizando o compilador **Roslyn** em tempo real (`CSharpScript.EvaluateAsync`), o sistema processa lógicas complexas de negócio diretamente na memória RAM, resolve variáveis dinâmicas e monta os comandos necessários. O histórico de logs é persistido localmente via **SQLite**.
2. **O Músculo (PowerShell):** Uma string limpa de comandos puros é enviada ao componente isolado `IExecutorBurro`. Este componente dispara uma instância oculta do `powershell.exe` forçando a comunicação em **UTF-8**, executando tarefas pesadas a nível de sistema operacional (downloads de ferramentas, varreduras de vírus, limpezas, etc.) e reportando o output em tempo real.

---

## 🛠️ Tecnologias Utilizadas (Stack)

* **Plataforma:** .NET 10 (C#)
* **Interface de Console:** Spectre.Console (Criação de menus ricos, spinners assíncronos e tabelas responsivas)
* **Compilação Dinâmica:** Microsoft.CodeAnalysis.CSharp.Scripting (Roslyn)
* **Banco de Dados:** SQLite (Via Entity Framework Core) para auditoria e logs de execução
* **Interpretador de Infraestrutura:** Windows PowerShell (Processos filhos assíncronos monitorados)

---

## 🚀 Recursos Implementados

* **Gerenciamento de Cancelamento Seguro:** Suporte nativo a `CancellationToken`. Se o usuário interromper uma operação com `Ctrl+C`, o motor dispara um encerramento cirúrgico que liquida toda a árvore de processos filhos do PowerShell, impedindo que tarefas invisíveis continuem rodando como "zumbis".
* **Saída Impecável (UTF-8):** Aplicação de uma "mordaça" na sessão do PowerShell para forçar a codificação em UTF-8, eliminando a quebra de acentuações em ambientes de MS-DOS legados.
* **Resiliência de Inicialização:** Sistema inteligente no `Program.cs` que gera automaticamente o diretório de `Scripts` se ele não existir e pausa a execução com alertas visuais caso esteja vazio, blindando o componente do Spectre Console contra quebras violentas.

---

## 📦 Como Compilar e Publicar (Publish)

Para gerar um utilitário tático independente e portátil (pronto para rodar a partir de um pendrive em computadores de clientes sem a necessidade do .NET Runtime instalado), utilize o comando de publicação direcionado para o projeto de console:

```bash
dotnet publish ConsoleApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
