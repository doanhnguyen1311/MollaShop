using SV21T1080007.DomainModels;

namespace SV21T1080007.Web.Models
{
    public class EmployeeSearchResult : PaginationSearchResult
    {
        public required List<Employee> Data { get; set; }
    }
}
