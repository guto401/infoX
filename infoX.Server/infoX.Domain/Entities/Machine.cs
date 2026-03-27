using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace infoX.Domain.Entities
{
    [Table("Machines")]
    public class Machine
    {
        [Key]
        public int MachineId { get; set; }

        [Required]
        [MaxLength(100)]
        public string MachineName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? OperatingSystem {  get; set; } = string.Empty;

        [Required]
        [MaxLength(45)]
        public string IpAddress {  get; set; } = string.Empty;

        [MaxLength(100)]
        public string CpuModel { get; set; } = string.Empty;

        public double TotalRamGb { get; set; }

        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
