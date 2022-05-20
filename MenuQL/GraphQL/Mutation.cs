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

namespace MenuQL.GraphQL
{
    public class Mutation
    {
        //MENU
        //Add Menu
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Menu> AddMenuAsync(
            InputMenu input,
            [Service] FoodDeliveryDBContext context)
        {
            var menu = new Menu
            {
                Name = input.Name,
                Stock = input.Stock,
                Price = input.Price,
                Created = DateTime.Now
            };

            var ret = context.Menus.Add(menu);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        //Menu by Id
        public async Task<Menu> GetMenuByIdAsync(
            int id,
            [Service] FoodDeliveryDBContext context)
        {
            var product = context.Menus.Where(o => o.Id ==id).FirstOrDefault();

            return await Task.FromResult(product);
        }

        //Update Menu
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Menu> UpdateMenuAsync(
            InputMenu input,
            [Service] FoodDeliveryDBContext context)
        {
            var product = context.Menus.Where(o => o.Id == input.Id).FirstOrDefault();
            if (product != null)
            {
                product.Name = input.Name;
                product.Stock = input.Stock;
                product.Price = input.Price;

                context.Menus.Update(product);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(product);
        }

        //Delete Menu
        [Authorize(Roles = new[] {"MANAGER"})]
        public async Task<Menu> DeleteMenuByIdAsync(
            int id,
            [Service] FoodDeliveryDBContext context)
        {
            var product = context.Menus.Where(o => o.Id == id).Include(u=>u.OrderDetails).FirstOrDefault();
            if (product != null)
            {
                context.Menus.Remove(product);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(product);
        }

    }
}
