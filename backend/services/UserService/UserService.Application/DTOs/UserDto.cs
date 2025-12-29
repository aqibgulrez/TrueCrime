namespace UserService.Application.DTOs;

public record UserDto(Guid Id, string FullName, string Email, string Role = "User", bool IsActive = true);
