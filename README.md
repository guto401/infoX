# 🛠️ InfoX — Sistema de Automação e Suporte Tático

📌 Utilitário avançado de infraestrutura, automação de rotinas e suporte de TI.

![GitHub](https://img.shields.io/badge/GitHub-8A2BE2?logo=github&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=flat&logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-5C2D91?logo=dotnet&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=flat&logo=windows&logoColor=white)
![PowerShell](https://img.shields.io/badge/PowerShell-5391FE?style=flat&logo=powershell&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat&logo=sqlite&logoColor=white)

[cite_start]O **InfoX** é o seu sistema de automação e suporte tático, focado em infraestrutura e TI[cite: 49].

---

## 🧠 Engenharia e Orquestração Híbrida

[cite_start]A arquitetura do projeto foi desenhada usando os conceitos profissionais da **Onion Architecture** (Arquitetura Cebola) em .NET 10[cite: 51]. [cite_start]O fluxo de execução adota a sacada da Orquestração Híbrida[cite: 52]:

1. [cite_start]**O Cérebro (C# + Roslyn):** O programa lê arquivos de script `.cs` físicos de uma pasta local chamada `Scripts`[cite: 52]. [cite_start]Ele usa o compilador **Roslyn** (`CSharpScript.EvaluateAsync`) em tempo real para processar lógicas complexas de C# na memória, injetar variáveis dinâmicas e gerar comandos mastigados[cite: 53]. [cite_start]A aplicação também utiliza um banco de dados SQLite para persistir o histórico e os logs de execução[cite: 54].
2. [cite_start]**O Músculo (PowerShell):** O C# cospe uma string limpa com comandos puros de PowerShell para um componente isolado chamado `IExecutorBurro`[cite: 55]. [cite_start]Ele abre um processo oculto do `powershell.exe`, aplica uma mordaça para forçar a comunicação em **UTF-8** (corrigindo bugs de acentuação no terminal) e executa a tarefa braçal no sistema operacional[cite: 56].

---

## ⚙️ Recursos Principais

* [cite_start]**Interface Avançada:** Para gerenciar a interface do terminal de forma rica e visual, o sistema utiliza o pacote `Spectre.Console`[cite: 57].
* [cite_start]**Resiliência de Inicialização:** Foi implementada uma trava de segurança que cria a pasta `Scripts` automaticamente caso ela não exista e pausa a tela com um aviso amigável se a mesma estiver vazia, impedindo o programa de quebrar na inicialização[cite: 58].
* [cite_start]**Arquitetura Reutilizável (Pronto para RMM):** Toda a lógica interna de `Application` e `Infrastructure` foi construída de forma desacoplada para ser reaproveitada futuramente em um projeto separado: um **Worker Service** que rodará em background como um agente de monitoramento remoto[cite: 59].

---

## 🚀 Roadmap e Próximos Passos

* [cite_start]**Cancelamento Seguro (Ctrl+C):** Atualmente, se você abortar uma execução no terminal, o painel em C# será fechado, mas o processo nativo do PowerShell continuará rodando invisível como um processo "zumbi" em segundo plano[cite: 60]. [cite_start]A próxima etapa arquitetural é estruturar o uso do `CancellationToken` para enviar um sinal de interrupção e matar toda a árvore de processos filhos no Windows de forma limpa[cite: 61].

---
[cite_start]desenvolvido por **[@guto_marmiroli](https://github.com/guto_marmiroli)** [cite: 62]
