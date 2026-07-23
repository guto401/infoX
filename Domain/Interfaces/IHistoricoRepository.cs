using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IHistoricoRepository
    {
        Task SalvarAsync(HistoricoExecucao historicoExecucao);

        Task<IEnumerable<HistoricoExecucao>> ObterHistoricoAsync();
    }
}
