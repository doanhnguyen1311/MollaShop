using SV21T1080007.DomainModels;

namespace SV21T1080007.Web.Models
{
    public class CategorySearchResult : PaginationSearchResult
    {
        public required List<Category> Data { get; set; }
    }
}
