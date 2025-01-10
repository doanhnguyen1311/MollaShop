using SV21T1080007.DomainModels;

namespace SV21T1080007.Web.Models
{
    public class ShipperSearchResult : PaginationSearchResult
    {
        public required List<Shipper> Data { get; set; }
    }
}
