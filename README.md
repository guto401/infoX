# 🛠️ InfoX — Sistema de Automação e Suporte Tático

📌 Utilitário avançado de infraestrutura, automação de rotinas e suporte de TI.

![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=flat&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=microsoftcsharp&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=flat&logo=windows&logoColor=white)
![PowerShell](https://img.shields.io/badge/PowerShell-5391FE?style=flat&logo=powershell&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat&logo=sqlite&logoColor=white)

O **InfoX** é um console interativo desenvolvido para administradores de sistemas e técnicos de campo executarem diagnósticos, automações e manutenções de forma centralizada, segura e com alto desempenho.

---

## 🧠 Engenharia e Orquestração Híbrida

O projeto adota os conceitos de **Onion Architecture** (Arquitetura Cebola) para desacoplar as regras de negócio dos componentes de infraestrutura, operando através de um motor de execução híbrido:

1. **O Cérebro (C# + Roslyn):** O sistema lê dinamicamente arquivos de script `.cs` locais e usa o compilador **Roslyn** (`CSharpScript.EvaluateAsync`) para processar lógicas complexas diretamente na memória RAM, injetar variáveis e estruturar o fluxo de trabalho. Os históricos de execução e logs são persistidos localmente via **SQLite**.
2. **O Músculo (PowerShell):** Uma interface isolada (`IExecutorBurro`) recebe os comandos prontos e dispara sessões ocultas do `powershell.exe`. O motor força nativamente a comunicação em **UTF-8**, eliminando erros de acentuação no console enquanto realiza tarefas pesadas a nível de sistema operacional.

---

## ⚙️ Recursos Principais

* **Interface Avançada:** Implementação com `Spectre.Console` para menus dinâmicos, tabelas limpas e indicadores visuais assíncronos (spinners) no terminal.
* **Cancelamento Seguro (Ctrl+C):** Gerenciamento preciso via `CancellationToken`. Caso a operação seja abortada, o sistema encerra cirurgicamente toda a árvore de processos filhos do PowerShell, evitando processos "zumbis" em background.
* **Resiliência de Inicialização:** Mecanismo inteligente que cria a pasta de `Scripts` caso não exista e pausa a execução com alertas visuais amigáveis se o diretório estiver vazio, blindando o terminal contra falhas.
* **Arquitetura Reutilizável:** Camadas internas estruturadas de forma independente (`Application` e `Infrastructure`), permitindo que este mesmo motor de automação seja facilmente portado para um **Worker Service** (Agente RMM em background) no futuro.

---
 desenvolvido por **[@guto_marmiroli](https://github.com/guto_marmiroli)**
