using Azure;
using Dapper;
using Microsoft.Identity.Client;
using SV21T1080007.DomainModels;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class ProductDAL : BaseDAL, IProductDAL
    {
        public ProductDAL(string connectionString) : base(connectionString) { }

        public long Add(Product data)
        {
            long id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS (SELECT * FROM Products WHERE ProductName = @ProductName)
                                SELECT -1
                            ELSE
                            BEGIN
                                INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                                VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, '', @IsSelling)

                                SELECT SCOPE_IDENTITY()
                            END
                            ";


                var parameter = new
                {
                    data.ProductName,
                    data.ProductDescription,
                    data.SupplierID,
                    data.CategoryID,
                    data.Unit,
                    data.Price,
                    data.IsSelling
                };

                id = connection.ExecuteScalar<long>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }


        public long AddAttribute(ProductAttribute data)
        {
            long id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS (
                                SELECT *
                                FROM ProductAttributes
                                WHERE AttributeName = @AttributeName and ProductID = @ProductID
                            )
                            SELECT -1
                            ELSE
                            BEGIN
                                INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder)
                                VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);

                                SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                            END
                            ";

                var parameter = new
                {
                    ProductID = data.ProductID,
                    AttributeName = data.AttributeName,
                    AttributeValue = data.AttributeValue,
                    DisplayOrder = data.DisplayOrder
                };

                id = connection.ExecuteScalar<long>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

        public long AddPhoto(ProductPhoto data)
        {
            long id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden)
                    VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                    UPDATE Products 
                    SET Photo = (SELECT TOP 1 Photo FROM ProductPhotos WHERE ProductID = @ProductID ORDER BY DisplayOrder)
                    WHERE ProductID = @ProductID
                                        ";

                var parameter = new
                {
                    ProductID = data.ProductID,
                    Photo = data.Photo,
                    Description = data.Description,
                    DisplayOrder = data.DisplayOrder,
                    IsHidden = data.IsHidden
                };

                id = connection.ExecuteScalar<long>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }


        public int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            searchValue = $"%{searchValue}%";
            int count = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT COUNT(*)
                            FROM Products
                            WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                            AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                            AND (@SupplierID = 0 OR SupplierId = @SupplierID)
                            AND (Price >= @MinPrice)
                            AND (@MaxPrice <= 0 OR Price <= @MaxPrice)";

                var parameter = new
                {
                    SearchValue = searchValue,
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                count = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return count;
        }


        public bool Delete(int productID)
        {
            bool res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from Products where ProductID = @ProductID";

                var parameter = new
                {
                    ProductID = productID,
                };

                res = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return res;
        }

        public bool DeleteAttribute(long attributeID)
        {
            bool res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";

                var parameter = new
                {
                    AttributeID = attributeID
                };

                res = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return res;
        }

        public bool DeletePhoto(long photoID)
        {
            var res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from ProductPhotos where PhotoID = @PhotoID";

                var parameter = new
                {
                    PhotoID = photoID,
                };
                res = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return res;
        }

        public Product? Get(int productID)
        {
            Product data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select * from Products where ProductID = @ProductID";

                var parameter = new
                {
                    ProductID = productID
                };
                data = connection.QueryFirstOrDefault<Product>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }

            return data;
        }

        public ProductAttribute GetAttribute(long attributeID)
        {
            ProductAttribute data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE AttributeID = @AttributeID";

                var parameter = new
                {
                    AttributeID = attributeID
                };

                data = connection.QueryFirstOrDefault<ProductAttribute>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }

            return data;
        }

        public ProductPhoto? GetPhoto(long photoID)
        {
            ProductPhoto photo = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select * from ProductPhotos where PhotoID = @PhotoID";
                var parameter = new
                {
                    PhotoID = photoID
                };
                photo = connection.QueryFirstOrDefault<ProductPhoto>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return photo;
        }

        public bool InUsed(int productID)
        {
            bool res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS (SELECT * FROM OrderDetails WHERE ProductID = @ProductID)
                               OR EXISTS (SELECT * FROM ProductPhotos WHERE ProductID = @ProductID)
                               OR EXISTS (SELECT * FROM ProductAttributes WHERE ProductID = @ProductID)
                            BEGIN
                               SELECT 1;
                            END
                            ELSE
                            BEGIN
                               SELECT 0;
                            END
                            ";
                var parameter = new
                {
                    ProductID = productID,
                };

                res = connection.ExecuteScalar<bool>(sql: sql, param: parameter, commandType: CommandType.Text);

                connection.Close();
            }
            return res;
        }

        public List<Product> List(int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            searchValue = $"%{searchValue}%";
            List<Product> products = new List<Product>();
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT *
                            FROM (
                            SELECT *,
                            ROW_NUMBER() OVER(ORDER BY ProductName) AS RowNumber
                            FROM Products
                            WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                            AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                            AND (@SupplierID = 0 OR SupplierId = @SupplierID)
                            AND (Price >= @MinPrice)
                            AND (@MaxPrice <= 0 OR Price <= @MaxPrice)
                            ) AS t
                            WHERE (@PageSize = 0)
                            OR (RowNumber BETWEEN (@Page - 1)*@PageSize + 1 AND @Page * @PageSize)";

                var parameter = new
                {
                    SearchValue = searchValue,
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Page = page,
                    PageSize = pageSize,
                };

                products = connection.Query<Product>(sql: sql, param: parameter, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return products;
        }

        public List<ProductAttribute> ListAttributes(int productID)
        {
            List<ProductAttribute> ds = new List<ProductAttribute>();
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE ProductID = @ProductID ORDER BY DisplayOrder";

                var parameter = new
                {
                    ProductID = productID
                };

                ds = connection.Query<ProductAttribute>(sql: sql, param: parameter, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return ds;
        }

        public List<ProductPhoto> ListPhotos(int productID)
        {
            List<ProductPhoto> ds = new List<ProductPhoto>();
            using (var connection = OpenConnection())
            {
                var sql = @"select * from ProductPhotos where ProductID = @ProductID
                            order by DisplayOrder";

                var parameter = new
                {
                    ProductID = productID,
                };

                ds = connection.Query<ProductPhoto>(sql: sql, param: parameter, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return ds;
        }

        public bool Update(Product data)
        {
            bool res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE Products
                            SET ProductName = @ProductName,
                                ProductDescription = @ProductDescription,
                                SupplierID = @SupplierID,
                                CategoryID = @CategoryID,
                                Unit = @Unit,
                                Price = @Price,
                                IsSelling = @IsSelling
                            WHERE ProductID = @ProductID
                            ";
                var parameter = new
                {
                    data.ProductName,
                    data.ProductDescription,
                    data.SupplierID,
                    data.CategoryID,
                    data.Unit,
                    data.Price,
                    data.IsSelling,
                    data.ProductID
                };

                res = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;

                connection.Close();
            }
            return res;
        }

        public bool UpdateAttribute(ProductAttribute data)
        {
            bool res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS (
                                SELECT *
                                FROM ProductAttributes
                                WHERE AttributeName = @AttributeName and ProductID = @ProductID
                            )
                            SELECT -1
                            ELSE
                            BEGIN
                            UPDATE ProductAttributes
                            SET 
                                ProductID = @ProductID,
                                AttributeName = @AttributeName,
                                AttributeValue = @AttributeValue,
                                DisplayOrder = @DisplayOrder
                            WHERE AttributeID = @AttributeID
                            END";

                var parameter = new
                {
                    AttributeID = data.AttributeID,
                    ProductID = data.ProductID,
                    AttributeName = data.AttributeName,
                    AttributeValue = data.AttributeValue,
                    DisplayOrder = data.DisplayOrder
                };

                res = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return res;
        }

        public bool UpdatePhoto(ProductPhoto data)
        {
            bool res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"
                            UPDATE ProductPhotos
                            SET 
                                ProductID = @ProductID,
                                Photo = @Photo,
                                Description = @Description,
                                DisplayOrder = @DisplayOrder,
                                IsHidden = @IsHidden
                            WHERE PhotoID = @PhotoID;
                            UPDATE Products 
                            SET Photo = (SELECT TOP 1 Photo FROM ProductPhotos WHERE ProductID = @ProductID ORDER BY DisplayOrder)
                            WHERE ProductID = @ProductID";
                var parameter = new
                {
                    PhotoID = data.PhotoID,
                    ProductID = data.ProductID,
                    Photo = data.Photo,
                    Description = data.Description,
                    DisplayOrder = data.DisplayOrder,
                    IsHidden = data.IsHidden
                };
                res = connection.Execute(sql, parameter, commandType: CommandType.Text) > 0;
                connection.Close();

            }
            return res;
        }


        public Category GetCategory(int cateID, long productID)
        {
            Category category = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT c.*
                    FROM Categories c
                    JOIN Product p ON p.CategoryID = c.CategoryID
                    WHERE p.ProductID = @ProductID AND c.CategoryID = @CategoryID";

                var parameter = new
                {
                    CategoryID = cateID, // Truyền đúng tham số CategoryID
                    ProductID = productID
                };

                category = connection.QuerySingleOrDefault<Category>(sql, parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return category;
        }

        public Supplier GetSupplier(int supplierID, long ProductID)
        {
            throw new NotImplementedException();
        }

        public List<Product> GetProductByKeyword(string keyword)
        {
            var list = new List<Product>();
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT TOP 6 *
                            FROM Products
                            WHERE (@SearchValue = N'' OR ProductName LIKE '%' + @SearchValue + '%')
                            ORDER BY ProductName;
                            ";

                var parameter = new
                {
                    SearchValue = keyword,
                };

                list = connection.Query<Product>(sql, parameter, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }
    }
}
