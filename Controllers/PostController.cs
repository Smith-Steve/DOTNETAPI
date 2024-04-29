using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DotnetAPI.Controllers
{
    //Attributes.
    [Authorize]
    [ApiController]
    //All of the routes inside of this controller will be based off of what we have here.
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        //Dapper
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }
        [HttpGet("Post")]
        public IEnumerable<Post> GetPosts()
        {
            string SqlQuery = "Select [PostId], [UserId],[PostTitle],[PostContent],[PostCreated],[PostUpdated] From TutorialAppSchema.Posts";
            return _dapper.LoadData<Post>(SqlQuery);
        }

        [HttpGet("PostSingle/{postId}")]
        public Post GetPostSingle(int postId)
        {
            string SqlQuery = $"Select [PostId], [UserId],[PostTitle],[PostContent],[PostCreated],[PostUpdated] From TutorialAppSchema.Posts WHERE PostId = {postId.ToString()}";
            return _dapper.LoadSingle<Post>(SqlQuery);
        }

        [HttpGet("PostsByUser/{UserId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string SqlQuery = $"Select [PostId], [UserId],[PostTitle],[PostContent],[PostCreated],[PostUpdated] From TutorialAppSchema.Posts WHERE UserId = {userId.ToString()}";
            return _dapper.LoadData<Post>(SqlQuery);
        }

        [HttpGet("PostsMyUser/{UserId}")]
        public IEnumerable<Post> GetMyPosts()
        {
            string SqlQuery = @"SELECT 
                                [PostId], [UserId], [PostTitle], [PostContent], [PostCreated], [PostUpdated]
                                FROM TutorialAppSchema.Posts WHERE UserId = " + this.User.FindFirst("UserId")?.Value;
            
            return _dapper.LoadData<Post>(SqlQuery);
        }

        [HttpPost("Post")]
        public IActionResult AddPost(PostsToAddDTO postToAdd)
        {
            string sql = @"
            INSERT INTO TutorialAppSchema.Posts(
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]) VALUES (" + this.User.FindFirst("userId")?.Value
                + ",'" + postToAdd.PostTitle
                + "','" + postToAdd.PostContent
                + "', GETDATE(), GETDATE() )";
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to create new post!");
        }

        [HttpPut("Post")]
        public IActionResult EditPost(PostsToEditDTO postToEdit)
        {

            Console.WriteLine(postToEdit);
            string sql = @"
            UPDATE TutorialAppSchema.Posts 
                SET PostContent = '" + postToEdit.PostContent + 
                "', PostTitle = '" + postToEdit.PostTitle + 
                @"', PostUpdated = GETDATE()
                    WHERE PostId = " + postToEdit.PostId.ToString() +
                    " AND UserId = " + this.User.FindFirst("userId")?.Value;

            Console.WriteLine(sql);
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to edit post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM TutorialAppSchema.Posts 
                WHERE PostId = " + postId.ToString()+
                    "AND UserId = " + this.User.FindFirst("userId")?.Value;

            
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }
    }
}