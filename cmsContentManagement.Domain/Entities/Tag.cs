using System.ComponentModel.DataAnnotations;

namespace cmsContentManagement.Domain.Entities;

public class Tag
{
    [Key]
    public Guid TagId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Content> Contents { get; set; } = new List<Content>();
}
