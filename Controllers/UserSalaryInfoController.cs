using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserSalaryInfoController : ControllerBase
    {
        DataContextDapper _dapper;
        public UserSalaryInfoController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }
        [HttpGet("UserSalaryInfo")]
        public IEnumerable<UserSalary> GetAllUserSalaries()
        {
            string sqlGetAllQuery = "Select [UserId], [Salary] from TutorialAppSchema.UserSalary";
            IEnumerable<UserSalary> users = _dapper.LoadData<UserSalary>(sqlGetAllQuery);
            return users;
        }

        [HttpGet("GetUserSalary/{userId}")]
        public UserSalary GetUserSalary(int userId)
        {
            string sqlGetSingleQuery = $"SELECT [UserId], [Salary] from TutorialAppSchema.UserSalary WHERE UserId = {userId.ToString()}";
            UserSalary userSalary = _dapper.LoadSingle<UserSalary>(sqlGetSingleQuery);
            return userSalary;
        }

        [HttpPut("EditSalary")]
        public IActionResult PutUserSalary(UserSalary updatedUserSalary)
        {
            string sqlUpdateUserSalary = $"UPDATE TutorialAppSchema.UserSalary SET Salary = {updatedUserSalary.Salary} WHERE UserId = {updatedUserSalary.UserId.ToString()}";
            if(_dapper.ExecuteSql(sqlUpdateUserSalary))
            {
                return Ok(updatedUserSalary);
            }
            throw new Exception("Updating User Salary Failed On Save");
        }

    [HttpPost("UserSalary")]
    public IActionResult PostUserSalary(UserSalary userSalaryForInsert)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.UserSalary (
                UserId,
                Salary
            ) VALUES (" + userSalaryForInsert.UserId.ToString()
                + ", " + userSalaryForInsert.Salary
                + ")";

        if (_dapper.ExecuteSqlWithRowCount(sql) > 0)
        {
            return Ok(userSalaryForInsert);
        }
        throw new Exception("Adding User Salary failed on save");
    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sqlDelete = $"DELETE FROM TutorialAppSchema.UserSalary Where UserId = {userId.ToString()}";
        if(_dapper.ExecuteSql(sqlDelete))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete User");
    }
    }
}