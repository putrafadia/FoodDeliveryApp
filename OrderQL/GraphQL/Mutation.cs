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

namespace OrderQL.GraphQL
{
    public class Mutation
    {
        //Order
        //Add Order
        [Authorize(Roles = new[] {"BUYER"})]
        public async Task<Order> AddOrderAsync(
            InputOrder input,
            [Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                var transaction = context.Database.BeginTransaction();
                try
                {
                    var order = new Order
                    {
                        UserId = user.Id,
                        Code = input.Code,
                        Status = false,
                        StartDate = DateTime.Now
                    };

                    for (int i = 0; i < input.OrderDetails.Count; i++)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.Id,
                            FoodId = input.OrderDetails[i].FoodId,
                            Quantity = input.OrderDetails[i].Quantity
                        };
                        order.OrderDetails.Add(orderDetail);
                    }

                    var ret = context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    transaction.Commit();

                    return ret.Entity;
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            return new Order();
        }

        //Add Courier to Order
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Order> AddCourierOrderAsync(
            AddCourierOrder input,
            [Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var order = context.Orders.Where(o => o.Id == input.Id).Include(d=>d.OrderDetails)
               .Include(c => c.User).FirstOrDefault();

            if (order == null)
            {
                var transaction = context.Database.BeginTransaction();
                try
                {
                    order.CourierId = input.CourierId;

                    var ret = context.Orders.Update(order);
                    await context.SaveChangesAsync();
                    transaction.Commit();

                    return ret.Entity;
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            return new Order();
            
        }

        //Update Order by Courier
        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<Order> UpdateOrderAsync(
            UpdateOrder input,
            [Service] FoodDeliveryDBContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();

            var order = context.Orders.Where(o => o.Id == input.Id).Include(d => d.OrderDetails)
               .Include(c => c.User).FirstOrDefault();

            var courier = context.Couriers.Where(o => o.UserId == user.Id).FirstOrDefault();

            if (order != null && order.CourierId == courier.Id)
            {
                var transaction = context.Database.BeginTransaction();
                try
                {
                    order.Status = input.Status;
                    order.EndDate = DateTime.Now;

                    var ret = context.Orders.Update(order);
                    await context.SaveChangesAsync();
                    transaction.Commit();

                    return ret.Entity;
                }
                catch
                {
                    transaction.Rollback();
                }
                
            }
            return new Order { CourierId = null, Code = "Null", Id = 0, UserId = 0  };          
        }

        ////Delete Order
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Order> DeleteOrderAsync(
            int id,
            [Service] FoodDeliveryDBContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).Include(n=>n.OrderDetails).FirstOrDefault();
            if (order != null)
            {
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
            }
            else
            {
                return new Order { CourierId = null, Code = "Null", Id = 0, UserId = 0 };
            }
            return order;
        }

    }
}
