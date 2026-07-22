// =============================================================================
// CAMADA: Application (Regras de negócio — depende apenas do Domain)
// ARQUIVO: IExecutorBurro.cs
// =============================================================================
// Esta é uma das peças mais importantes da arquitetura inteira.
//
// CONCEITO — Por que uma interface aqui?
// A Application precisa "executar comandos no sistema operacional", mas ela
// NÃO pode saber COMO isso é feito. Ela não pode importar System.Diagnostics
// nem abrir um Process diretamente — isso seria infraestrutura entrando no
// meio das regras de negócio.
//
// A solução da Onion Architecture é: a Application DEFINE O CONTRATO
// (a interface), e a Infrastructure IMPLEMENTA o contrato (a classe concreta).
// A Application fala com a interface; nunca com a implementação.
//
// RESULTADO PRÁTICO:
// - Hoje a implementação abre um powershell.exe (ExecutorPowerShell.cs)
// - Amanhã, para o RMM, podemos criar um ExecutorSSH.cs ou ExecutorWinRM.cs
// - O GerenciadorScripts.cs não precisará mudar nenhuma linha de código!
//
// O nome "Burro" é intencional e descreve o design: este executor não pensa,
// não valida, não interpreta. Ele recebe uma string e executa. A inteligência
// toda fica no GerenciadorScripts (que usa o Roslyn).
// =============================================================================

namespace Application.Interfaces
{
    public interface IExecutorBurro
    {
        // Executa um bloco de comandos e retorna o output completo como string.
        //
        // Parâmetros:
        //   scriptConteudo — a string de comandos PowerShell pura, gerada pelo Roslyn
        //   onLineRead     — callback opcional chamado a cada linha recebida em tempo real.
        //                    Permite que a UI mostre o output antes da execução terminar.
        //                    O "?" indica que o parâmetro é nullable (pode ser null).
        Task<string> ExecutarAsync(string scriptConteudo, Action<string>? onLineRead = null);
    }
}
