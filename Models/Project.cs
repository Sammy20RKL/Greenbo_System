using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class Project
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    // Foreign key
    public long? CompanyID { get; set; }

    [StringLength(50)]
    public string? ProjectName { get; set; }

    public string? FileUpload { get; set; }

    public int? Quantity { get; set; }

    [StringLength(50)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [StringLength(10)]
    public string Status { get; set; } = "New";

    // Navigation property
    [ForeignKey("CompanyID")]
    public Company? Company { get; set; }
  }
}
