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
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var menu = context.Menus;
            if (menu != null)
            {
                if (adminRole.Value == "MANAGER" || adminRole.Value == "BUYER")
                {
                    return menu;
                }
            }
            return new List<Menu>().AsQueryable();
        }


    }
}
