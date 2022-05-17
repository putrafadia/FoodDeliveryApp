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
                    return context.Users.Select(p => new UserData()
                    {
                        Id = p.Id,
                        FullName = p.FullName,
                        Email = p.Email,
                        Username = p.Username
                    });
                }
            }

            return new List<UserData>().AsQueryable();
        }
        
    }
}
