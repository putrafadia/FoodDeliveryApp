using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using System;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Models.Models;
using Microsoft.EntityFrameworkCore;

namespace UserQL.GraphQL
{
    public class Mutation
    {
        //USERS
        //Register User
        public async Task<UserData> RegisterUserAsync(
            RegisterUser input,
            [Service] FoodDeliveryDBContext context)
        {
            var user = context.Users.Where(o=>o.Username == input.UserName).FirstOrDefault();
            if(user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password)
            };

            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData { 
                Id=newUser.Id,
                Username=newUser.Username,
                Email =newUser.Email,
                FullName=newUser.FullName
            });
        }

        //Login User
        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] FoodDeliveryDBContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null,null,"Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password,user.Password);
            if (valid)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.Id == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o=>o.Id == userRole.RoleId).FirstOrDefault();
                    if(role!=null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,   
                    claims: claims,
                    signingCredentials: credentials
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }

        //Change Password
        public async Task<User> ChangePasswordAsync(
            ChangePassword input,
            [Service] FoodDeliveryDBContext context)
        {
            
            var user = context.Users.Where(o=>o.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                bool valid = BCrypt.Net.BCrypt.Verify(input.OldPassword, user.Password);
                if (valid)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
                    //user.Username = user.Username;
                    //user.FullName = user.FullName;
                    //user.Email = user.Email;

                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                    return await Task.FromResult(new User { Password = "Berhasil di ganti" });
                }
                else
                {
                    return await Task.FromResult(new User { Username = "Salah", Password = "Salah" });
                }
            }

            return await Task.FromResult(user);

        }

        //Delete user
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> DeleteUserByIdAsync(
        int id,
        [Service] FoodDeliveryDBContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

    }
}
