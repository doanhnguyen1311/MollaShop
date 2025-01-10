using Microsoft.Extensions.Caching.Memory;
using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;

namespace SV21T1080007.Web.Models
{
    public class Cache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly List<Order> listOrders;

        // Constructor để khởi tạo với IMemoryCache và danh sách đơn hàng
        public Cache(IMemoryCache memoryCache, List<Order> listOrder)
        {
            _memoryCache = memoryCache;
            listOrders = listOrder;
        }

        // Phương thức để cache dữ liệu
        public void CacheData()
        {
            // Tạo một key cache
            string cacheKey = "orderListCacheKey";

            // Kiểm tra nếu dữ liệu đã có trong cache
            if (!_memoryCache.TryGetValue(cacheKey, out List<Order> cachedOrders))
            {
                // Dữ liệu chưa có trong cache, cần phải tính toán và thêm vào cache
                cachedOrders = listOrders;

                // Lưu vào cache với thời gian hết hạn là 5 phút
                _memoryCache.Set(cacheKey, cachedOrders, TimeSpan.FromMinutes(5));
                Console.WriteLine("Data added to cache.");
            }
            else
            {
                Console.WriteLine("Data retrieved from cache.");
                // Dữ liệu đã có trong cache
                foreach (var order in cachedOrders)
                {
                    Console.WriteLine($"Order ID: {order.OrderID}, Customer: {order.CustomerName}");
                }
            }
        }
    }
}
