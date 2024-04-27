using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [ApiController]
    [Route("controller")]
    public class UserJobInfoController : ControllerBase
    {
        DataContextDapper _dapper;
        public UserJobInfoController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        [HttpGet("TestConnection/UserJobInfo")]
        public DateTime TestConnection()
        {
            return _dapper.LoadSingle<DateTime>("Select GETDATE()");
        }

        [HttpGet("GetUserJobInfo/All")]
        public IEnumerable<UserJobInfo> GetUserJobInfo()
        {
            string sqlGetUserJobInfoQuery = @"Select * From TutorialAppSchema.UserJobInfo";
            IEnumerable<UserJobInfo> users = _dapper.LoadData<UserJobInfo>(sqlGetUserJobInfoQuery);
            return users;
        }
        [HttpGet("GetUserJobInfo/{userInfoId}")]
        public UserJobInfo GetSingleUserJobInfo(int userId)
        {
            string sqlGetSingleUserJobInfoQuery = $"Select [UserId], [JobTitle], [Department] From TutorialAppSchema.UserJobInfo Where UserId = {userId}";
            UserJobInfo userInfo = _dapper.LoadSingle<UserJobInfo>(sqlGetSingleUserJobInfoQuery);
            return userInfo;
        }

        [HttpPut("EditUserJobInfo")]
        public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
        {
            string sqlUpdateUserJobInfo = $"UpdateTutorialAppSchema.UserJobInfo SET [JobTitle] = {userJobInfo.JobTitle}, [Department] = {userJobInfo.Department}";
            if(_dapper.ExecuteSql(sqlUpdateUserJobInfo))
            {
                return Ok(); 
            }
            throw new Exception($"Failed To Update {userJobInfo.UserId}'s Job Info");
        }
        
        [HttpPost("AddUserJobInfo")]
        public IActionResult PostUserJobInfo(UserJobInfo userJobInfo)
        {
            string sql = @"
                INSERT INTO TutorialAppSchema.UserJobInfo (
                    UserId,
                    Department,
                    JobTitle
                ) VALUES (" + userJobInfo.UserId
                    + ", '" + userJobInfo.Department
                    + "', '" + userJobInfo.JobTitle
                    + "')";
            if(_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to Add User's Job Info");
        }

        [HttpDelete("DeleteUserJobInfo/{userId}")]
        public IActionResult DeleteUserJobInfo(int userId)
        {
            string deleteSqlString = $"DELETE FROM TutorialAppSchema.UserJobInfo Where UserId = {userId.ToString()}";
            if(_dapper.ExecuteSql(deleteSqlString))
            {
                return Ok();
            }
            throw new Exception("Failed to Delete User");
        }
    }
    
}