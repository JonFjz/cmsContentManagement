namespace cmsContentManagement.Application.DTO;

public class ContentDTO
{
    public Guid ContentId { get; set; }
    public string? AssetUrl  { get; set; }
    public string? Status { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? RichContent {  get; set; }
    public Guid UserId  { get; set; }
    
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    
    public List<TagDTO> Tags { get; set; } = new List<TagDTO>();
}