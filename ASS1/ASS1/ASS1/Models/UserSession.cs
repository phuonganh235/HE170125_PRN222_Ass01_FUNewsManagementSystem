namespace ASS1.Models
{
    public class UserSession
    {
        public short AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountEmail { get; set; } = string.Empty;
        public int AccountRole { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
    }
}
