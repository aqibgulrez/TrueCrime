using UserService.Domain.Enums;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public class User : BaseEntity
{
    public Email Email { get; private set; }
    public string FullName { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    private User() { } // EF Core

    public User(string fullName, Email email)
    {
        FullName = fullName;
        Email = email;
        Role = UserRole.User;
        IsActive = true;
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
