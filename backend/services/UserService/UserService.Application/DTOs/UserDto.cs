namespace UserService.Application.DTOs;

public record UserDto(Guid Id, string FullName, string Email, string Role = "User", bool IsActive = true);

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);

public record CreateUserRequestDto(string Email, string FullName, string Password);
public record LoginRequestDto(string Email, string Password);

public record ForgotPasswordRequestDto(string Email);
public record ResetPasswordRequestDto(string Token, string NewPassword);
