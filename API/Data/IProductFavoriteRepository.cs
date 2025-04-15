using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public interface IProductFavoriteRepository
    {
        Task<bool> IsFavorite(int productId, string userId);
        Task AddFavorite(int productId, string userId);
        Task RemoveFavorite(int productId, string userId);
        Task<List<ProductDto>> GetUserFavorites(string userId);
    }
}
