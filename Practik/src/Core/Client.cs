namespace Core
{
    public class Client
    {
        public int ClientId { get; set; }
        public int OwnerUserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string DisplayName => $"{FirstName} {LastName} ({PhoneNumber})";
    }
}
