using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using TFCiclo.Api.Controllers;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Repositories;
using TFCiclo.Data.Services;

namespace TFCiclo.Tests.Controllers
{
    [TestClass]
    public class AuthControllerTests
    {
        private MockRepository mockRepository;

        private Mock<UserRepository> mockUserRepository;
        private Mock<Logger> mockLogger;
        private Mock<IConfiguration> mockConfiguration;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockUserRepository = this.mockRepository.Create<UserRepository>();
            this.mockLogger = this.mockRepository.Create<Logger>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private AuthController CreateAuthController()
        {
            return new AuthController(
                this.mockUserRepository.Object,
                this.mockLogger.Object,
                this.mockConfiguration.Object);
        }

        [TestMethod]
        public async Task Login_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var authController = this.CreateAuthController();
            ApiObjectRequest dto = null;
            CancellationToken cToken = default(global::System.Threading.CancellationToken);

            // Act
            var result = await authController.Login(
                dto,
                cToken);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task Register_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var authController = this.CreateAuthController();
            ApiObjectRequest dto = null;
            CancellationToken cToken = default(global::System.Threading.CancellationToken);

            // Act
            var result = await authController.Register(
                dto,
                cToken);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
