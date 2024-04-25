
using System.Data;
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;


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
                                    [Active] From TutorialAppSchema.Users";
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
    }
}