using SV21T1080007.DomainModels;

namespace SV21T1080007.Web.Models
{
    public class CustomerSearchResult : PaginationSearchResult
    {
        public required List<Customer> Data { get; set; }
    }
}
