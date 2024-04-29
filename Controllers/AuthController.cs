using System.Data;
using System.Text;
using System.Security.Cryptography;
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace DotnetAPI.Controllers
{
    //"Authorize" is something that we can use to tell the controller that we want to ensure that they are authorized
    // when they access this controller.
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
            _configuration = configuration;
        }
        [AllowAnonymous]
        //This tells our API - just the one with the attribute - is allowed to recieve an anonymous request.
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDTO userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                //We need to send an inquiry to ensure that our new user exists.
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" +
                    userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                //This is where we begin configuring the combination, and storage, or our users password so that
                //their information is secure. It largely uses different independent methods of storage to store different
                //"keys" (We'll call them - I don't know if that is the official term) that scramble the original string
                //of Characters.
                //We do not actually store the password, what we do is we store a scrambled version of the password,
                //And we create seperate independent sources that 'unscramble' the users password so they can get in.
                if(existingUsers.Count() == 0)
                {
                    //This is to set the size of the bytes to 128. Not sure what the '8' does.
                    byte[] passwordSalt = new byte[128/8];
                    //We create a random number generator. We use that to create a byte array, and store it inside our password salt.
                    using(RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt); //
                    }

                    //We generated a numebr in our appsettings.json file. This file contains a random string (we just typed it into the keyboard)
                    //that will serve as our 'scrambling' - and I assume - unscrambling key.

                    //This is where we access the password.

                    //Our password key does not live in the database. It only lives in the application - or the middle layer. This
                    //Is done so that if a hacker accesses the database, they will not actually have the password unless they can
                    //Access the application as well.

                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

                    //The "@" symbol is the way you create a variable in sql.
                    string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth 
                                        ([Email], [PasswordHash], [PasswordSalt])
                                        VALUES
                                        ('" + userForRegistration.Email + "', @PasswordHash, @PasswordSalt)";
                    
                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    if(_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users(
                                [FirstName],
                                [LastName],
                                [Email],
                                [Gender],
                                [Active]
                            ) VALUES (" +
                                "'" + userForRegistration.FirstName + 
                                "', '" + userForRegistration.LastName +
                                "', '" + userForRegistration.Email + 
                                 "', '" + userForRegistration.Gender +
                            "', 1)";
                        if(_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed To Add User.");
                    }

                    throw new Exception("Failed to Register User.");
                }
                throw new Exception("User Already Exists.");
            }
            throw new Exception("Passwords do not match.");
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDTO userForLogin)
        {
            string sqlForHashAndSalt = @"SELECT 
                [PasswordHash], [PasswordSalt]
                 FROM TutorialAppSchema.Auth WHERE Email = '" +
                userForLogin.Email + "'";
            Console.WriteLine(sqlForHashAndSalt);
            UserForLoginConfirmationDTO userForConfirmation = _dapper.LoadSingle<UserForLoginConfirmationDTO>(sqlForHashAndSalt);
            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index]!= userForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect Password");
                }
            }

                        string userIdSql = @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" +
                userForLogin.Email + "'";

            int userId = _dapper.LoadSingle<int>(userIdSql);
            return Ok(new Dictionary<string,string> {
                {"token", CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";

            //We want to check and see if our user exists.
            //Our SQL string.
            string userIdSql = $"SELECT userId FROM TutorialAppSchema.Users WHERE UserId = {userId}";
            
            //Create a new interger value.
            int userIdFromDb = _dapper.LoadSingle<int>(userIdSql); //This will return our userId

            //If they do exist then we know that this token that gave us the UserId is valid.

            //We can then create a new token and return it to the user so their user session is still valid.
            
            return Ok(new Dictionary<string,string> {
                {"token", CreateToken(userIdFromDb)}
            });
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _configuration.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

                    //Our password key does not live in the database. It only lives in the application - or the middle layer. This
                    //Is done so that if a hacker accesses the database, they will not actually have the password unless they can
                    //Access the application as well.

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                //This is where we set the number of times that a password is hashed.
                iterationCount: 100000,
                numBytesRequested: 256/8
            );
        }

        private string CreateToken(int userId)
        {
            Claim[] claims = new Claim[] {
                new Claim("userId", userId.ToString()),
            };

            string? tokenKeyString = _configuration.GetSection("AppSettings:TokenKey").Value;
            Console.WriteLine("Token Key Print Out: ");

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    tokenKeyString != null ? tokenKeyString : ""
                )
            );
            SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}