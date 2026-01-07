using System.ComponentModel.DataAnnotations;

namespace cmsContentManagement.Domain.Entities;

public class Content
{
    [Key]
    public Guid ContentId { get; set; }
    [Url]
    public string? AssetUrl  { get; set; }
    public string? Title { get; set; }
    public string? RichContent {  get; set; }

    public string Status { get; set; } = "New";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UserId { get; set; }
    
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}