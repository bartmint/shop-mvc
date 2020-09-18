﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Domain.Models;
using OnlineShop.Infrastructure.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ShoppingCartRepository:IShoppingCartRepository
    {
        private readonly ApplicationDb _ctx;
        private readonly ISessionSettings _session;
        private static string ShoppingCartId { get; set; }
        public IQueryable<ShoppingCartItem> ShoppingCartItems { get; set; }


        public ShoppingCartRepository(ApplicationDb ctx, ISessionSettings session)
        {
            _ctx = ctx;
            _session = session;
        }

        public void AddToCart(Product product, int quantity)
        {
            //setting Id
            ShoppingCartId = _session.OnGet();

            //retrieve product from database
            //ShoppingCartId = GetCartId();

            var cartItems = 
                _ctx.CartItems.FirstOrDefault(
                c => c.Product.Id == product.Id
                && c.CartId == ShoppingCartId);
            //create new product if no cart item exists
            if (cartItems == null)
            {
                cartItems = new ShoppingCartItem
                {
                    CartId=ShoppingCartId,
                    Product=product,
                    Quantity=quantity,
                };
                _ctx.CartItems.Add(cartItems);
            }
            else
            {
                cartItems.Quantity++;
            }
            _ctx.SaveChanges();
        }
        public void Dispose()
        {
            if (_ctx != null)
            {
                _ctx.Dispose();
            }
        }

        public int RemoveFromCart(Product product)
        {
            var shoppingCartItem =
                _ctx.CartItems.SingleOrDefault(
                    s => s.Product.Id == product.Id
                    && s.CartId == ShoppingCartId);
            var localAmount = 0;
            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Quantity > 1)
                {
                    shoppingCartItem.Quantity--;
                    localAmount = shoppingCartItem.Quantity;
                }
                else
                {
                    _ctx.CartItems.Remove(shoppingCartItem);
                }
            }
            _ctx.SaveChanges();
            return localAmount;
        }

        public IQueryable<ShoppingCartItem> GetShoppingCartItems()
        {
            return ShoppingCartItems ?? (
                ShoppingCartItems =
                _ctx.CartItems
                .Where(c => c.CartId == ShoppingCartId)
                .Include(s => s.Product)
                .ThenInclude(p => p.Paths)
                .Include(a => a.Product.Ammount));
                 
        }

        public async Task ClearCart()
        {
            var cartItems = _ctx
                .CartItems
                .Where(s => s.CartId == ShoppingCartId);
            _ctx.CartItems.RemoveRange(cartItems);

           await _ctx.SaveChangesAsync();
        }

        public decimal GetShoppingCartTotal()
        {
            var total = _ctx.CartItems
                .Where(s => s.CartId == ShoppingCartId)
                .Select(s => s.Product.Value * s.Quantity).Sum();
            return total;
        }
        public async Task<int> GetShoppingCartAmmount()
        {
            var total = await _ctx.CartItems.Where(s => s.CartId == ShoppingCartId).CountAsync();
            return total;
        }
    }
}
