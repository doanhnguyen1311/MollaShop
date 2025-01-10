using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.DataLayers
{
    public interface IProductDAL 
    {
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng trên mỗi trang (0 nếu không phân trang)</param>
        /// <param name="searchValue">Tên mặt hàng cần tìm (rỗng nếu không tìm kiếm)</param>
        /// <param name="categoryID">Mã loại hàng cần tìm (0 nếu không tìm theo loại hàng)</param>
        /// <param name="supplierID">Mã nhà cung cấp cần tìm (0 nếu không tìm theo nhà cung cấp)</param>
        /// <param name="minPrice">Mức giá nhỏ nhất trong khoảng giá cần tìm (0 nếu không hạn chế mức giá nhỏ nhất)</param>
        /// <param name="maxPrice">Mức giá lớn nhất trong khoảng giá cần tìm (0 nếu không hạn chế mức giá lớn nhất)</param>
        /// <returns></returns>
        List<Product> List(int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0);

        /// <summary>
        /// Đếm số lượng mặt hàng tìm kiếm được
        /// </summary>
        int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0);

        /// <summary>
        /// Lấy thông tin mặt hàng theo mã hàng
        /// </summary>
        Product Get(int productID);

        /// <summary>
        /// Bổ sung mặt hàng mới (hàm trả về mã của mặt hàng được bổ sung)
        /// </summary>
        long Add(Product data);

        /// <summary>
        /// Cập nhật thông tin mặt hàng
        /// </summary>
        bool Update(Product data);

        /// <summary>
        /// Xóa mặt hàng
        /// </summary>
        bool Delete(int productID);

        /// <summary>
        /// Kiểm tra xem mặt hàng hiện có đơn hàng liên quan hay không?
        /// </summary>
        bool InUsed(int productID);

        // Methods for handling product photos
        List<ProductPhoto> ListPhotos(int productID);
        ProductPhoto GetPhoto(long photoID);
        long AddPhoto(ProductPhoto data);
        bool UpdatePhoto(ProductPhoto data);
        bool DeletePhoto(long photoID);

        // Methods for handling product attributes
        List<ProductAttribute> ListAttributes(int productID);
        ProductAttribute GetAttribute(long attributeID);
        long AddAttribute(ProductAttribute data);
        bool UpdateAttribute(ProductAttribute data);
        bool DeleteAttribute(long attributeID);

        Category GetCategory(int cateID,long productID);

        Supplier GetSupplier(int supplierID, long ProductID);

        List<Product> GetProductByKeyword(string keyword);
    }

}
