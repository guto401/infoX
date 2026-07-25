using Domain.Enums;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class HistoricoExecucaoDto
    {
        public string NomeScript { get; set; } = string.Empty;
        public DateTime DataOriginal { get; set; }
        public string DataExecucao => DataOriginal.ToString("dd/MM/yyyy");
        public StatusEnum Status { get; set; }
        public string OutputLog { get; set; } = string.Empty;
    }
}
