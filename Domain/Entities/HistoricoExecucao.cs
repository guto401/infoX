// =============================================================================
// CAMADA: Domain (O núcleo da cebola — não depende de NADA externo)
// ARQUIVO: HistoricoExecucao.cs
// =============================================================================
// Esta entidade representa um registro de auditoria: cada vez que um script
// é executado, um HistoricoExecucao é criado e salvo no banco de dados.
//
// Isso é importante para rastreabilidade — se algo der errado, você pode
// consultar o banco e ver exatamente qual script rodou, quando, e qual foi
// o output completo (incluindo erros).
// =============================================================================

using Domain.Enums;

namespace Domain.Entities
{
    public class HistoricoExecucao
    {
        // Chave primária gerada automaticamente pelo banco.
        public int Id { get; set; }

        // Nome do arquivo .cs que foi executado (ex: "LimpezaTemp.cs").
        public string NomeScript { get; set; } = string.Empty;

        // Momento exato em que a execução aconteceu.
        // Útil para ordenar o histórico e identificar execuções recentes.
        public DateTime DataExecucao { get; set; }

        // O resultado da execução: Concluido, Erro, Cancelado, etc.
        // No AppDbContext, configuramos para salvar como string ("Concluido")
        // em vez de inteiro (0), o que facilita ler o banco diretamente.
        public StatusEnum Status { get; set; }

        // Todo o texto que o PowerShell jogou na tela durante a execução.
        // Inclui tanto o output normal quanto as mensagens de erro.
        public string OutputLog { get; set; } = string.Empty;

        // Construtor vazio exigido pelo Entity Framework Core (mesma razão
        // explicada em Usuario.cs).
        public HistoricoExecucao() { }

        // Construtor de conveniência
        public HistoricoExecucao(string nome, StatusEnum status, string resultado)
        {
            NomeScript = nome;  
            Status = status;    
            OutputLog = resultado; 
        }

        public HistoricoExecucao(HistoricoExecucao historicoExecucao)
        {

        }
    }
}
