using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace MenuQL.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<Menu> GetMenus([Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (adminRole.Value == "MANAGER" || adminRole.Value == "BUYER")
                {
                    return context.Menus;
                }
            }
            return new List<Menu>().AsQueryable();
        }


    }
}
