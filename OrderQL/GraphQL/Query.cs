using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace OrderQL.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<Order> GetOrder([Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role).FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            var courier = context.Couriers.Where(o => o.UserId == user.Id).FirstOrDefault();
            if (user != null)
            {
                if (adminRole.Value == "MANAGER")
                {
                    return context.Orders;
                }
                if (adminRole.Value == "BUYER")
                {
                    return context.Orders.Where(o => o.UserId == user.Id).Include(d => d.OrderDetails);
                }
                if (adminRole.Value == "COURIER")
                {
                    return context.Orders.Where(o => o.CourierId == courier.Id).Include(d => d.OrderDetails);
                }
            }
            return new List<Order>().AsQueryable();
        }


    }
}
