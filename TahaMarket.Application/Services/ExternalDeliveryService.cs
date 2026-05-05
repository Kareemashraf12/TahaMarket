using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

namespace TahaMarket.Application.Services
{
    public class ExternalDeliveryService
    {
        private readonly ApplicationDbContext _context;

        public ExternalDeliveryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Store يطلب دليفري
        public async Task<object> RequestDelivery(Guid storeId, string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new Exception("Address is required");

            var request = new ExternalDeliveryRequest
            {
                StoreId = storeId,
                Address = address
            };

            _context.ExternalDeliveryRequests.Add(request);
            await _context.SaveChangesAsync();

            return new
            {
                message = "Request sent",
                requestId = request.Id
            };
        }

        // admin see external delivery requests
        public async Task<List<ExternalDeliveryRequestResponseDto>> GetRequests()
        {
            return await _context.ExternalDeliveryRequests
                .Include(x => x.Store)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ExternalDeliveryRequestResponseDto
                {
                    Id = x.Id,
                    StoreName = x.Store.Name,
                    Address = x.Address,
                    IsAssigned = x.IsAssigned,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }
    }
}
