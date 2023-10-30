using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        string sql = @"EXEC TutorialAppSchema.spPost_Get @UserId= " + userId;

        return _dapper.LoadData<Post>(sql);
    }

    [HttpPut("UpsertPost")]
    public IActionResult UpsertPosts(PostUpsert postToUpsert)
    {
        string? userId = this.User.FindFirst("userId")?.Value;

        string sqlString =
            @"EXEC TutorialAppSchema.spPosts_Upsert
                @UserId ="
            + userId
            + ", @PostTitle ='"
            + postToUpsert.PostTitle
            + "', @PostContent ='"
            + postToUpsert.PostContent
            + "'";

        if (postToUpsert.PostId > 0)
        {
            sqlString += ", @PostId = " + postToUpsert.PostId;
        }

        if (!_dapper.ExecuteSql(sqlString))
        {
            return StatusCode(420, "Unable to add new post!");
        }
        return Ok();
    }

    [HttpDelete("Delete")]
    public IActionResult DeletePost(int postId)
    {
        string? userId = this.User.FindFirst("userId")?.Value;

        string sqlString =
            @"EXEC TutorialAppSchema.spPost_Delete @PostId = "
            + postId.ToString()
            + ", @UserId = "
            + userId;

        if (!_dapper.ExecuteSql(sqlString))
        {
            return StatusCode(420, "Unable to DELETE post!");
        }

        return Ok();
    }
}
