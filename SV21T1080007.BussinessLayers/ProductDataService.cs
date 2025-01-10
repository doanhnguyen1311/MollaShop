using SV21T1080007.DataLayers;
using SV21T1080007.DataLayers.SQLServer;
using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.BusinessLayers
{
    public static class ProductDataService
    {
        private static readonly IProductDAL productDB;

        static ProductDataService()
        {
            productDB = new ProductDAL(Configuration.ConnectionString);
        }
        /// <summary>
        /// List khong phan trang
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public static List<Product> ListProductsNonPagination(string searchValue = "")
        {
            return null;
        }
        /// <summary>
        /// List co phan trang
        /// </summary>
        /// <param name="rowCount"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <param name="categoryID"></param>
        /// <param name="supplierID"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<Product> ListProducts(out int rowCount, int page = 1, int pageSize=0,string searchValue="", 
            int categoryID = 0, int supplierID = 0, decimal minPrice=0, decimal maxPrice = 0)
        {
            rowCount = productDB.Count(searchValue, categoryID, supplierID, minPrice, maxPrice);
            return productDB.List(page, pageSize, searchValue, categoryID, supplierID, minPrice, maxPrice);
        }
        /// <summary> Lấy thông tin 1 mặt hàng theo mã mặt hàng </summary>
        public static Product? GetProduct(int productID)
        {
            return productDB.Get(productID);
        }

        /// <summary> </summary>
        public static long AddProduct(Product data)
        {
            return productDB.Add(data);
        }

        /// <summary> </summary>
        public static bool UpdateProduct(Product data)
        {
            return productDB.Update(data);
        }

        /// <summary> </summary>
        public static bool DeleteProduct(int productID)
        {
            return productDB.Delete(productID);
        }

        /// <summary> </summary>
        public static bool InUsedProduct(int productID)
        {
            return productDB.InUsed(productID);
        }

        /// <summary> </summary>
        public static List<ProductPhoto> ListPhotos(int productID)
        {
            return productDB.ListPhotos(productID);
        }

        /// <summary> </summary>
        public static ProductPhoto? GetPhoto(long photoID)
        {
            return productDB.GetPhoto(photoID);
        }

        /// <summary> </summary>
        public static long AddPhoto(ProductPhoto data)
        {
            return productDB.AddPhoto(data);
        }

        /// <summary> </summary>
        public static bool UpdatePhoto(ProductPhoto data)
        {
            return productDB.UpdatePhoto(data);
        }

        /// <summary> </summary>
        public static bool DeletePhoto(long photoID)
        {
            return productDB.DeletePhoto(photoID);
        }

        /// <summary> </summary>
        public static List<ProductAttribute> ListAttributes(int productID)
        {
            return productDB.ListAttributes(productID);
        }

        /// <summary> </summary>
        public static ProductAttribute? GetAttribute(int attributeID)
        {
            return productDB.GetAttribute(attributeID);
        }

        /// <summary> </summary>
        public static long AddAttribute(ProductAttribute data)
        {
            return productDB.AddAttribute(data);
        }

        /// <summary> </summary>
        public static bool UpdateAttribute(ProductAttribute data)
        {
            return productDB.UpdateAttribute(data);
        }

        /// <summary> </summary>
        public static bool DeleteAttribute(long attributeID)
        {
            return productDB.DeleteAttribute(attributeID);
        }

        public static Category GetCategory(int cateID,long productID)
        {
            return productDB.GetCategory(cateID, productID);
        }

        public static Supplier GetSupplier()
        {
            return null;
        }

        public static List<Product> GetListByKeyword(string keyword)
        {
            return productDB.GetProductByKeyword(keyword);
        }
    }



}
