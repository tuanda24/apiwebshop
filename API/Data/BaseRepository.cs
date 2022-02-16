﻿using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class BaseRepository
    {
        public DataContext DataContext { get; }
        public IMapper Mapper { get; }
        public IPhotoService PhotoService { get; }

        public BaseRepository(DataContext dataContext, IMapper mapper, IPhotoService photoService)
        {
            DataContext = dataContext;
            Mapper = mapper;
            PhotoService = photoService;
        }

        #region Save

        public async Task<bool> SaveChanges()
        {
            return await DataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return DataContext.ChangeTracker.HasChanges();
        }

        #endregion

        #region User

        public async Task<string> GetUserNameById(int id)
        {
            return await DataContext.Users.Where(u => u.Id == id)
                .Select(u => u.UserName)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetUserIdByUserName(string userName)
        {
            return await DataContext.Users.Where(u => u.UserName == userName.ToLower())
                .Select(u => u.Id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> UserExist(int id)
        {
            return await DataContext.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> UserExist(string userName)
        {
            return await DataContext.Users.AnyAsync(u => u.UserName == userName.ToLower());
        }

        public async Task<Location> GetUserLocation(int userId)
        {
            if (userId == 0)
                return null;

            return await DataContext.Addresses.Where(a => a.User.Id == userId)
                .Include(a => a.Location.Parent.Parent.Parent)
                .Select(a => a.Location)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UserHasAddress(int userId)
        {
            if (userId == 0) return false;
            return await DataContext.Users.AnyAsync(u => u.AddressId != null);
        }

        public async Task<string> GetUserAddressName(int userId)
        {
            if (userId == 0) return null;
            return await DataContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Address.Name)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Product

        public async Task<bool> IsInterStateDelivery(int userId, int storeId)
        {
            if (userId == 0) return false;

            var userLocationId = await DataContext.Addresses.Where(a => a.User.Id == userId)
                .Select(a => a.Location.Parent.ParentId)
                .FirstOrDefaultAsync();

            return await DataContext.Stores
                .Where(s => s.Id == storeId && s.Address.Location.Parent.ParentId == userLocationId)
                .AnyAsync();
        }

        public async Task UpdateProductAvailability(List<int> productIds)
        {
            foreach (var productId in productIds)
            {
                var product = await DataContext.Products.Where(p => p.Id == productId).FirstAsync();
                if (await DataContext.StoreItems.Where(si => si.ProductId == productId && si.Available > 0)
                        .AnyAsync()) continue;
                product.Available = false;
                await SaveChanges();
            }
        }

        #endregion

        #region Account

        public async Task<Transaction> ProcessTransaction(int from, int to, double amount, string description)
        {
            var transaction = new Transaction
            {
                Amount = amount,
                Date = DateTime.UtcNow,
                Description = description,
                FromId = from,
                ToId = to
            };

            var fromAcc = await DataContext.Accounts.SingleAsync(a => a.Id == from);
            var toAcc = await DataContext.Accounts.SingleAsync(a => a.Id == to);

            if (fromAcc.Balance < amount)
                throw new HttpException("Insufficient balance to cover this transactions.");

            fromAcc.Balance -= amount;
            toAcc.Balance += amount;

            return transaction;
        }

        #endregion

        #region Photo

        public async Task UpdatePhoto(IFormFile file, Photo photo, bool profile = false)
        {
            var result = await PhotoService.UploadImage(file, profile);
            if (result.Error != null) throw new HttpException(result.Error.Message, StatusCodes.Status500InternalServerError);
            photo.PublicId = result.PublicId;
            photo.Url = result.SecureUrl.AbsoluteUri;
        }

        public async Task DeletePhoto(Photo photo)
        {
            await PhotoService.DeleteImage(photo.PublicId);
            photo.PublicId = null;
            photo.Url = null;
        }

        public async Task DeletePhoto(string publicId)
        {
            await PhotoService.DeleteImage(publicId);
        }

        #endregion

        #region Location

        public async Task<List<LocationDto>> GetChildLocations(string parentName, string childType)
        {
            return await DataContext.Locations
                .Where(l => l.Parent.Name == parentName && l.Type == childType)
                .ProjectTo<LocationDto>(Mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<LocationDto>> GetChildLocations(int parentId, string childType)
        {
            if (parentId == 0) return await GetChildLocations("India", childType);
            return await DataContext.Locations
                .Where(l => l.ParentId == parentId && l.Type == childType)
                .ProjectTo<LocationDto>(Mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LocationListDto> GetLocations(int areaId)
        {
            return await DataContext.Locations
                .Where(l => l.Id == areaId && l.Type == LocationType.Area.ToString())
                .Select(l => new LocationListDto
                {
                    AreaId = areaId,
                    Areas = l.Parent.Children.Select(a => new LocationDto { Id = a.Id, Name = a.Name }),
                    Cities = l.Parent.Parent.Children.Select(c => new LocationDto { Id = c.Id, Name = c.Name }),
                    States = l.Parent.Parent.Parent.Children.Select(s => new LocationDto { Id = s.Id, Name = s.Name })
                })
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}
