using Domain.Enums;
namespace Domain.Entities
{
    public class HistoricoExecucao
    {
        public int Id { get; set; }
        public string NomeScript { get; set; } = string.Empty;
        public DateTime DataExecucao { get; set; }
        public StatusEnum Status { get; set; }
        public string OutputLog { get; set; } = string.Empty;
    }
}
