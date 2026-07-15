using Domain.Entities;


namespace Application.Interfaces
{
    public interface IHistoricoRepository
    {
        Task SalvarAsync(HistoricoExecucao historicoExecucao);
        Task<IEnumerable<HistoricoExecucao>> ObterHistoricoAsync();
    }
}
