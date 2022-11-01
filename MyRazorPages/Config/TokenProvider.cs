using MyRazorPages.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.EntityFrameworkCore;

namespace MyRazorPages.Config
{
    public class TokenProvider
    {
        private readonly PRN221DBContext _prn221DBContext;

        public TokenProvider(PRN221DBContext prn221DBContext)
        {
            _prn221DBContext = prn221DBContext;
        }

        public  string LoginUser(string UserID, string Password)
        {
            //Get user details for the user who is trying to login
            var user =  _prn221DBContext.Accounts.Include(a => a.RoleNavigation).FirstOrDefault(x => x.AccountId == Int32.Parse(UserID));

            //Authenticate User, Check if it’s a registered user in Database 
            if (user == null)
                return null;

            //If it is registered user, check user password stored in Database
            //For demo, password is not hashed. It is just a string comparision 
            //In reality, password would be hashed and stored in Database. 
            //Before comparing, hash the password again.
            if (Password.Equals(user.Password))
            {
                //Authentication successful, Issue Token with user credentials 
                //Provide the security key which is given in 
                //Startup.cs ConfigureServices() method 
                var key = Encoding.ASCII.GetBytes
                ("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");
                //Generate Token for user 
                var JWToken = new JwtSecurityToken(
                    issuer: "http://localhost:5000/",
                    audience: "http://localhost:5000/",
                    claims: (IEnumerable<Claim>)GetUserClaims(user),
                    notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                    expires: new DateTimeOffset(DateTime.Now.AddDays(1)).DateTime,
                    //Using HS256 Algorithm to encrypt Token  
                    signingCredentials: new SigningCredentials
                    (new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                );
                var token = new JwtSecurityTokenHandler().WriteToken(JWToken);
                return token;
            }
            else
            {
                return null;
            }
        }


        //Using hard coded collection list as Data Store for demo. 
        //In reality, User data comes from Database or other Data Source 
        private IEnumerable GetUserClaims(Account user)
        {
            IEnumerable claims = new Claim[]
                    {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("USERID", user.AccountId.ToString()),
                    new Claim(ClaimTypes.Role, user.RoleNavigation.Name),
                    };
            return claims;
        }
    }
}
