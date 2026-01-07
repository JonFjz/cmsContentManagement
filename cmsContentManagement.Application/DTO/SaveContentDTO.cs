namespace cmsContentManagement.Application.DTO;

public class SaveContentDTO
{
    public string? AssetUrl  { get; set; }
    public string? Title { get; set; }
    public string? RichContent {  get; set; }
    public string? CategoryName { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}
