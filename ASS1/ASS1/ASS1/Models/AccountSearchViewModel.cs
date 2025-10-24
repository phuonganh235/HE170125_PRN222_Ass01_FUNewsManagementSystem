using ASS1.DAL.Models;

namespace ASS1.Models
{
    public class AccountSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public int? RoleFilter { get; set; }
        public List<SystemAccount> Accounts { get; set; } = new List<SystemAccount>();
        public List<RoleOption> RoleOptions { get; set; } = new List<RoleOption>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
