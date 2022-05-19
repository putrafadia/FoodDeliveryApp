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

        //Add User Role
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<UserRole> AddUserRoleAsync(
          InputUserRole input,
          [Service] FoodDeliveryDBContext context)
        {
            var userRole = new UserRole
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
            };
            var ret = context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        //Add Data Couriers
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Courier> AddCourierAsync(
          InputCourier input,
          [Service] FoodDeliveryDBContext context)
        {
            var user = context.Users.Where(o => o.Id == input.UserId).FirstOrDefault();
            if (user == null) return new Courier();

            var courier = new Courier
            {
                CourierName = user.FullName,
                PhoneNumber = input.PhoneNumber,
                UserId = input.UserId,
            };
            var ret = context.Couriers.Add(courier);
            await context.SaveChangesAsync();

            return ret.Entity;
        }


        //PROFILE
        //Add Profile
        public async Task<Profile> AddProfileAsync(
           InputProfile input,
           [Service] FoodDeliveryDBContext context)
        {
            var profile = new Profile
            {
                UserId = input.UserId,
                Name = input.Name,
                Address = input.Address,
                City = input.City,
                Phone = input.Phone,               
            };
            var ret = context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        //Update Profile
        public async Task<Profile> UpdateProfileAsync(
            InputProfile input,
            [Service] FoodDeliveryDBContext context)
        {
            var profile = context.Profiles.Where(o => o.Id == input.Id).FirstOrDefault();
            if (profile != null)
            {
                profile.Name = input.Name;
                profile.Address = input.Address;
                profile.City = input.City;
                profile.Phone = input.Phone;

                context.Profiles.Update(profile);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(profile);
        }

        //Delete Profile
        public async Task<Profile> DeleteProfileByIdAsync(
            int id,
            [Service] FoodDeliveryDBContext context)
        {
            var profile = context.Profiles.Where(o => o.Id == id).FirstOrDefault();
            if (profile != null)
            {
                context.Profiles.Remove(profile);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(profile);
        }
    }
}
