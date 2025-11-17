namespace cmsContentManagment.Application.DTO;

public class ContentDTO
{
    public Guid ContentId { get; }
    public string? AssetUrl  { get; set; }
    public string? Title { get; set; }
    public string? RichContent {  get; set; }
    public Guid UserId  { get; set; }
}