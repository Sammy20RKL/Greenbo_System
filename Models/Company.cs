using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class Company
  {
    [Key]
    public long ID { get; set; }

    [StringLength(250)]
    public string? CompanyName { get; set; }

    [StringLength(20)]
    public string? RegistrationNumber { get; set; }

    [StringLength(20)]
    public string? KraPin { get; set; }

    [StringLength(50)]
    public string? ContactAddress { get; set; }

    [StringLength(50)]
    public string? Designation { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(50)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? Status { get; set; }

    // Navigation property
    public ICollection<Project> Projects { get; set; }
  }
}
