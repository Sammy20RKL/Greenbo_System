using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  [Table("Users")]
  public class User
  {
    [Key]
    public long ID { get; set; }

    [StringLength(50)]
    public string? Name { get; set; }
  }
}
