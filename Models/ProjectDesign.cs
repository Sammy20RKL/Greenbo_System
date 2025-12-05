using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class ProjectDesign
  {
    [Key]
    public long ID { get; set; }

    [ForeignKey("Project")]
    public long ProjectID { get; set; }

    public string? FileUpload { get; set; }

    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Project? Project { get; set; }
  }
}
