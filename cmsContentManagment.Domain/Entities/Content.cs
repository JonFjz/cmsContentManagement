using System.ComponentModel.DataAnnotations;

namespace cmsContentManagment.Domain.Entities;

public class Content
{
    [Key]
    public Guid ContentId { get; }
    [Url]
    public string? AssetUrl  { get; set; }
    public string? Title { get; set; }
    public string? RichContent {  get; set; }

    public string Status { get; set; } = "New";
    [Required]
    public Guid UserId { get; set; }
}