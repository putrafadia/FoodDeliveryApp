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
            if (user == null) return new Order();

            var order = new Order
            {
                UserId = input.UserId,
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

            return ret.Entity;
        }

        ////Menu by Id
        //public async Task<Menu> GetMenuByIdAsync(
        //    int id,
        //    [Service] FoodDeliveryDBContext context)
        //{
        //    var product = context.Menus.Where(o => o.Id ==id).FirstOrDefault();

        //    return await Task.FromResult(product);
        //}

        ////Update Menu
        //[Authorize(Roles = new[] { "MANAGER" })]
        //public async Task<Menu> UpdateMenuAsync(
        //    InputOrder input,
        //    [Service] FoodDeliveryDBContext context)
        //{
        //    var product = context.Menus.Where(o => o.Id == input.Id).FirstOrDefault();
        //    if (product != null)
        //    {
        //        //product.Name = input.Name;
        //        //product.Stock = input.Stock;
        //        //product.Price = input.Price;

        //        context.Menus.Update(product);
        //        await context.SaveChangesAsync();
        //    }


        //    return await Task.FromResult(product);
        //}

        ////Delete Menu
        //[Authorize(Roles = new[] {"MANAGER"})]
        //public async Task<Menu> DeleteMenuByIdAsync(
        //    int id,
        //    [Service] FoodDeliveryDBContext context)
        //{
        //    var product = context.Menus.Where(o => o.Id == id).FirstOrDefault();
        //    if (product != null)
        //    {
        //        context.Menus.Remove(product);
        //        await context.SaveChangesAsync();
        //    }


        //    return await Task.FromResult(product);
        //}

    }
}
