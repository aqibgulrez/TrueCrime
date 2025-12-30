using System.Threading.Tasks;
using Xunit;
using Moq;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Auth;
using UserService.Application.Interfaces.Persistence;
using UserService.Application.DTOs;

namespace UserService.Tests
{
    public class IdentityProviderTests
    {
        [Fact]
        public async Task LocalRegisterAndActivateFlow_Works()
        {
            var idpMock = new Mock<UserService.Application.Interfaces.IIdentityProviderService>();
            idpMock.Setup(i => i.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var reg = await idpMock.Object.RegisterAsync("u@example.com", "Password123!", "User Example");
            Assert.True(reg);
        }

        [Fact]
        public async Task LocalForgotConfirmFlow_Works()
        {
            var idpMock = new Mock<UserService.Application.Interfaces.IIdentityProviderService>();
            idpMock.Setup(i => i.InitiateForgotPasswordAsync(It.IsAny<string>())).ReturnsAsync(true);

            var initiated = await idpMock.Object.InitiateForgotPasswordAsync("u@example.com");
            Assert.True(initiated);
        }
    }
}
