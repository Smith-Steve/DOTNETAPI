
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;


namespace DotnetAPI.Controllers
{
     [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public UserController()
        {

        }
        [HttpGet("test")]
        public string[] Test()
        {
            string[] responseArray = new string[]
            {
                "Test1",
                "Test2"
            };
            return responseArray;
        }
    }
}