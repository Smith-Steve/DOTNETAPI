
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;


namespace DotnetAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        DataContextDapper _dapper;
        public UserController(IConfiguration configuration)
        {
            //Constructors should be set to public or else the instantiation of the class
            //will cause an error.
            Console.WriteLine(configuration.GetConnectionString("DefaultConnection"));
            _dapper = new DataContextDapper(configuration);
        }
        [HttpGet("TestConnection")]
        public DateTime TestConnection()
        {
            return _dapper.LoadSingle<DateTime>("SELECT GETDATE()");
        }
        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            string sqlGetQuery = @"Select [UserId],
                                    [FirstName],
                                    [LastName],
                                    [Email],
                                    [Gender],
                                    [Active] From TutorialAppSchema.Users
                                    Order By UserId Desc";
            IEnumerable<User> users = _dapper.LoadData<User>(sqlGetQuery);
            return users;
        }

        [HttpGet("GetUser/{userId}")]
        public User GetSingleUser(int userId)
        {
            string sqlGetQuery = @"Select [UserId],
                                    [FirstName],
                                    [LastName],
                                    [Email],
                                    [Gender],
                                    [Active] 
                                    From TutorialAppSchema.Users
                                    Where UserId = " + userId.ToString();
            User user = _dapper.LoadSingle<User>(sqlGetQuery);
            return user;
        }

        [HttpPut("EditUser")]
        //IActionResult acknowledges that a successful request was put through, or a failed request did not make it.
        public IActionResult EditUser(User user)
        {
            string sqlUpdate = @"
                UPDATE TutorialAppSchema.Users
                    SET [FirstName] = '" + user.FirstName + 
                        "', [LastName] = '" + user.LastName +
                        "', [Email] = '" + user.Email + 
                        "', [Gender] = '" + user.Gender + 
                        "', [Active] = '" + user.Active + 
                    "' WHERE UserId = " + user.UserId;
            Console.WriteLine(sqlUpdate);
            if(_dapper.ExecuteSql(sqlUpdate))
            {
                return Ok();
            }
            throw new Exception("Failed to Update User");
        }
        [HttpPost("AddUser")]
        public IActionResult PostUser(UserDTO user)
        {
            string sqlPost = @"INSERT INTO TutorialAppSchema.Users(
                                [FirstName],
                                [LastName],
                                [Email],
                                [Gender],
                                [Active]
                            ) VALUES (" +
                                "'" + user.FirstName + 
                                "', '" + user.LastName +
                                "', '" + user.Email + 
                                 "', '" + user.Gender + 
                                "', '" + user.Active +  
                            "')";
            if(_dapper.ExecuteSql(sqlPost))
            {
                return Ok();
            }
            throw new Exception("Failed to Add User");  
        }

        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string Sql = $"DELETE FROM TutorialAppSchema.Users Where UserId = {userId.ToString()}";
            if(_dapper.ExecuteSql(Sql))
            {
                return Ok();
            }
            throw new Exception("Failed to Delete User");
        }
    }
};