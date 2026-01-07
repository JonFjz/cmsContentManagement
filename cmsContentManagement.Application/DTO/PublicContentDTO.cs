namespace cmsContentManagement.Application.DTO;

public class PublicContentDTO
{
    public Guid ContentId { get; set; }
    public string? AssetUrl { get; set; }
    public string? Title { get; set; }
    public string? RichContent { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public Guid UserId { get; set; }
    
    public CategoryDTO? Category { get; set; }
    public List<TagDTO> Tags { get; set; } = new List<TagDTO>();
}
