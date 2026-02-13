using AdessoWorldLeague.Core.Entities;

namespace AdessoWorldLeague.Data.Documents;

public class UserDocument : BaseDocument
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = "User";
}
