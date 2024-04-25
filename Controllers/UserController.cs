
using System.Data;
using DotnetAPI.Data;
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
        [HttpGet("GetUsers/{testValue}")]
        public string[] GetUsers(string testValue)
        {
            string[] responseArray = new string[]
            {
                "Test1",
                "Test2",
                testValue
            };
            return responseArray;
        }
    }
}