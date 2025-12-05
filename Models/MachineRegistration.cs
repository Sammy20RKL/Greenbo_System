using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class MachineRegistration
  {
    [Key]
    public long ID { get; set; }

    [StringLength(50)]
    public string? MachineName { get; set; }

    [StringLength(250)]
    public string? MachinePurpose { get; set; }

    [StringLength(50)]
    public string? MachineManufacturer { get; set; }

    [StringLength(20)]
    public string? MachineStatus { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string? CreatedBy { get; set; }
  }
}
