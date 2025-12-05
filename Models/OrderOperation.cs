using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  [Table("OrderOperations")]
  public class OrderOperation
  {
    [Key]
    public long ID { get; set; }

    // Foreign key for Project
    public long? ProjectID { get; set; }

    // Foreign key for MaterialList (Part)
    public int? PartNumber { get; set; }

    public int? Process { get; set; }

    public int? AssignedTo { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    // Navigation Properties - These create the actual relationships
    [ForeignKey("ProjectID")]
    public virtual Project? Project { get; set; }

    //  [ForeignKey("PartNumbe")]
    //  public virtual MaterialList? MaterialList { get; set; }
    //}
  }
}
