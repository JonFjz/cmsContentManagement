namespace cmsContentManagement.Application.DTO;

public class SaveContentDTO
{
    public string? AssetUrl { get; set; }
    public string? Title { get; set; }
    public string? RichContent {  get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<TagDTO> Tags { get; set; } = new List<TagDTO>();
}
