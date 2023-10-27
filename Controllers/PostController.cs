using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DotnetApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly DataContextDapper _dapper;

    public PostController(IConfiguration configuration)
    {
        _dapper = new(configuration);
    }

    [HttpGet("GetPosts")]
    public IEnumerable<Post> GetPosts(int? userId, int? postId, string? searchQuery)
    {
        string sql = @"EXEC TutorialAppSchema.spPost_Get";
        string parameters = "";

        if (userId is not null)
        {
            parameters += ", @UserId= " + userId.ToString();
        }
        if (postId is not null)
        {
            parameters += ", @PostId= " + postId.ToString();
        }
        if (!string.IsNullOrEmpty(searchQuery))
        {
            parameters += ", @SearchQuery= '" + searchQuery.ToString() + "'";
        }

        if (!string.IsNullOrEmpty(parameters))
        {
            sql += parameters[1..];
        }

        return _dapper.LoadData<Post>(sql);
    }

    [HttpGet("MyPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
        // this.User can be used to refer to the User from ControllerBase if we have multiple User's
        string? userId = this.User.FindFirst("userId")?.Value;
        string sql =
            @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated]
                FROM TutorialAppSchema.Posts
                    WHERE UserId = " + userId;

        return _dapper.LoadData<Post>(sql);
    }

    [HttpPost("Post")]
    public IActionResult AddPost(PostToAddDto postToAdd)
    {
        string? userId = this.User.FindFirst("userId")?.Value;

        string sqlString =
            @"
            INSERT INTO TutorialAppSchema.Posts(
            [UserId],
            [PostTitle],
            [PostContent],
            [PostCreated],
            [PostUpdated]
            ) VALUES ("
            + userId
            + ",'"
            + postToAdd.PostTitle
            + "','"
            + postToAdd.PostContent
            + "', GETDATE(), GETDATE())";

        if (!_dapper.ExecuteSql(sqlString))
        {
            return StatusCode(420, "Unable to add new post!");
        }
        return Ok();
    }

    [HttpPut("Post")]
    public IActionResult EditPost(PostToEditDto postToEdit)
    {
        string? userId = this.User.FindFirst("userId")?.Value;

        string sqlString =
            @"
            UPDATE TutorialAppSchema.Posts
            SET PostContent = '"
            + postToEdit.PostContent
            + "', PostTitle = '"
            + postToEdit.PostTitle
            + @"', PostUpdated = GETDATE()
            WHERE PostId = "
            + postToEdit.PostId.ToString()
            + "AND UserId = "
            + userId;

        if (!_dapper.ExecuteSql(sqlString))
        {
            return StatusCode(420, "Unable to Edit post!");
        }
        return Ok();
    }

    [HttpDelete("Post/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        string? userId = this.User.FindFirst("userId")?.Value;

        string sqlString =
            @"DELETE FROM TutorialAppSchema.Posts
            WHERE PostId = "
            + postId.ToString()
            + "AND UserId = "
            + userId;

        if (!_dapper.ExecuteSql(sqlString))
        {
            return StatusCode(420, "Unable to DELETE post!");
        }

        return Ok();
    }
}
