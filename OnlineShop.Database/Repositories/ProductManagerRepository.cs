﻿using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Domain.Models;
using OnlineShop.Infrastructure.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ProductManagerRepository : IProductManager
    {
        private readonly ApplicationDb _ctx;

        public ProductManagerRepository(ApplicationDb ctx)
        {
            _ctx = ctx;
        }
        public Task<int> CreateProduct(Product product)
        {
            _ctx.Products.Add(product);

            return _ctx.SaveChangesAsync();
        }

        public Task<int> DeleteProduct(Product product)
        {

            var entity = _ctx.Products.FirstOrDefault(p => p.Id == product.Id);
            _ctx.Products.Remove(entity);

            return _ctx.SaveChangesAsync();
        }

        public Product GetProductById(int id)
        {
            return _ctx.Products.Include(x => x.Paths).FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Product> ProductsE()
        {
            return _ctx.Products.Include(x => x.Paths);
        }

        public Task<int> UpdateProduct(Product product)
        {
            _ctx.Products.Update(product);
            return _ctx.SaveChangesAsync();
        }
    }
}
