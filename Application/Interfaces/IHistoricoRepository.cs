// =============================================================================
// CAMADA: Application (Regras de negócio — depende apenas do Domain)
// ARQUIVO: IHistoricoRepository.cs
// =============================================================================
// Contrato de persistência para os registros de auditoria.
// Segue o mesmo padrão do IUsuarioRepository — a Application define o contrato,
// a Infrastructure implementa.
//
// Note que este repositório é intencionalmente mais simples: o histórico só
// precisa ser salvo e consultado. Não há atualização ou remoção de registros
// (logs de auditoria geralmente são imutáveis por design).
// =============================================================================

using Domain.Entities;

namespace Application.Interfaces
{
    public interface IHistoricoRepository
    {
        // Salva um registro de execução no banco após cada script rodar.
        // Chamado no bloco "finally" do GerenciadorScripts para garantir
        // que o registro seja feito mesmo que ocorra uma exceção.
        Task SalvarAsync(HistoricoExecucao historicoExecucao);

        // Retorna todos os registros ordenados do mais recente para o mais antigo.
        // Útil para uma futura tela de "Histórico de Execuções".
        Task<IEnumerable<HistoricoExecucao>> ObterHistoricoAsync();
    }
}
