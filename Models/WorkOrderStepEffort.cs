using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  [Table("WorkOrderStepEffort")]
  public class WorkOrderStepEffort
  {
    [Key]
    public long EffortId { get; set; }

    [Required]
    [ForeignKey("OrderOperation")]
    public long OperationId { get; set; }

    [ForeignKey("MachineRegistration")]
    public long? MachineId { get; set; }

    public long? OperatorId { get; set; }

    public int? QuantityAssigned { get; set; }

    public int? QuantityCompleted { get; set; }

    [StringLength(20)]
    public string? UnitOfMeasure { get; set; }

    public int? StandardTimePerUnit { get; set; }

    public int? ExpectedDuration { get; set; }

    public int? ActualDuration { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Efficiency { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? MachineStartTime { get; set; }

    public DateTime? MachineEndTime { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public MachineRegistration? Machine { get; set; }

    public OrderOperation? OrderOperation { get; set; }
  }
}
