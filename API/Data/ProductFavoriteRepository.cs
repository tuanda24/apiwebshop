using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductFavorite = API.Entities.ProductFavorite;

namespace API.Data
{
    public class ProductFavoriteRepository : BaseRepository, IProductFavoriteRepository
{
        public ProductFavoriteRepository(DataContext dataContext, IMapper mapper, IPhotoService photoService, Microsoft.AspNetCore.Identity.UserManager<User> _userManager) : base(dataContext, mapper, photoService)
        {
        }

        public async Task<bool> IsFavorite(int productId, string userId)
        {
            return await DataContext.ProductFavorite.AnyAsync(x => x.ProductId == productId && x.UserId == userId);
        }

        public async Task AddFavorite(int productId, string userId)
        {
            var exists = await IsFavorite(productId, userId);
            if (exists) return;

            DataContext.ProductFavorite.Add(new ProductFavorite
            {
                ProductId = productId,
                UserId = userId,
                FavoritedAt = DateTime.UtcNow
            });

            await DataContext.SaveChangesAsync();
        }

        public async Task RemoveFavorite(int productId, string userId)
        {
            var favorite = await DataContext.ProductFavorite
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.UserId == userId);

            if (favorite != null)
            {
                DataContext.ProductFavorite.Remove(favorite);
                await DataContext.SaveChangesAsync();
            }
        }

        public async Task<List<ProductDto>> GetUserFavorites(string userId)
        {
            return await DataContext.ProductFavorite
                .Where(x => x.UserId == userId)
                .Select(x => new ProductDto
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Amount = x.Product.Amount,
                    //Brand = x.Product.Brand,
                    //Description = x.Product.Description
                })
                .ToListAsync();
        }
    }

}
