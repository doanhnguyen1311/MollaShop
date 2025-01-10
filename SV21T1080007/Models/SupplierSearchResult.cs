using SV21T1080007.DomainModels;

namespace SV21T1080007.Web.Models
{
    public class SupplierSearchResult : PaginationSearchResult
    {
        public required List<Supplier> Data { get; set; }
    }
}
