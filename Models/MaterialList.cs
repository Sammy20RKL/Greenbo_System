using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class MaterialList
  {
    [Key]
    public long ID { get; set; }

    [ForeignKey("Project")]
    public long? ProjectID { get; set; }

    public int? PartNumber { get; set; }

  
    public string? Description { get; set; }

    public string? Mark { get; set; }

    public string? Profile { get; set; }
    
    public int? Quantity { get; set; }

    public int? Length { get; set; }

    public decimal? UnitWeight { get; set; }

    public decimal? TotalWeight { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    // Navigation property
    public Project? Project { get; set; }
  }
}
