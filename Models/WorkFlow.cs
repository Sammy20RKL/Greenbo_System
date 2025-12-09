using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AspnetCoreMvcFull.Models
{
  [Table("Workflow")]
  public class WorkFlow
  {
    [Key]
    public long ID { get; set; }

    [Required]
    [StringLength(50)]
    public string? ProcessName { get; set; }


    [StringLength(250)]
    public string? Description { get; set; }

    [Required]
    [ForeignKey("Machine")]
    public long RequiredMachine { get; set; }


    // Computed column (ID + 1)
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public long NextStep { get; private set; }


    // Navigation
    public MachineRegistration? Machine { get; set; }
  }
}
