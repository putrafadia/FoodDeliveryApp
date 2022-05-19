using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace UserQL.GraphQL
{
    public class Query
    { 
        [Authorize]
        public IQueryable<UserData> GetUsers([Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var users = context.Users;
            if (users != null)
            {
                if (adminRole.Value == "ADMIN")
                {
                    return context.Users.Include(o => o.UserRoles).ThenInclude(r => r.Role).Select(p => new UserData()
                    {
                        Id = p.Id,
                        FullName = p.FullName,
                        Email = p.Email,
                        Username = p.Username,
                        UserRoles = p.UserRoles
                    });
                }
            }
            return new List<UserData>().AsQueryable();
        }

        [Authorize]
        public IQueryable<UserRole> GetUserRoleCouriers([Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var users = context.UserRoles.Include(u=>u.User).Where(o=>o.RoleId == 2);
            if (users != null)
            {
                if (adminRole.Value == "MANAGER")
                {
                    return users;
                }
            }

            return new List<UserRole>().AsQueryable();
        }

        [Authorize]
        public IQueryable<Courier> GetCouriers([Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (adminRole.Value == "MANAGER")
                {
                    return context.Couriers;
                }

            }

            return new List<Courier>().AsQueryable();
        }

        [Authorize]
        public IQueryable<Profile> GetProfiles([Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check admin role ?
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (adminRole.Value == "ADMIN")
                {
                    return context.Profiles;
                }
                var profiles = context.Profiles.Where(o => o.UserId == user.Id);
                return profiles.AsQueryable();
            }

            return new List<Profile>().AsQueryable();
        }

    }
}
