namespace DotnetApi.Models;

public partial class PostUpsert
{
    public int? PostId { get; set; }
    public string PostTitle { get; set; } = "";
    public string PostContent { get; set; } = "";
}
