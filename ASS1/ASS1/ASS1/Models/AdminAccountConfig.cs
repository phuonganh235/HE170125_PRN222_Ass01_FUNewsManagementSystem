namespace ASS1.Models
{
    public class AdminAccountConfig
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public int AccountRole { get; set; }
    }
}
