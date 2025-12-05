using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class MachineAllocation
  {
    [Key]
    public long ID { get; set; }

    public long? MachineID { get; set; }
    public long? ProjectID { get; set; }

    [StringLength(50)]
    public string UserID { get; set; }

    public DateTime Datex { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("MachineID")]
    public MachineRegistration? Machine { get; set; }

    [ForeignKey("ProjectID")]
    public Project? Project { get; set; }
  }
}
