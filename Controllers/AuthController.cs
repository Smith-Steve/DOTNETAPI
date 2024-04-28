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

namespace DotnetAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
            _configuration = configuration;
        }

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

            // string? tokenKeyString = _configuration.GetSection("AppSettings:Token").Value;
            // SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
            //     Encoding.UTF8.GetBytes(
            //         tokenKeyString != null ? tokenKeyString : ""
            //     )
            // );

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