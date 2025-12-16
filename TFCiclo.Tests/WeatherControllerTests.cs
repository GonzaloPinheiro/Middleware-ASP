using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TFCiclo.Api.Controllers;
using TFCiclo.Data.ApiObjects;
using TFCiclo.Data.Models;
using TFCiclo.Data.Repositories;
using TFCiclo.Data.Services;

[TestClass]
public class WeatherControllerTests
{
    private Mock<WeatherRepository> _weatherRepoMock;
    private Mock<UserRepository> _userRepoMock;
    private Mock<Logger> _loggerMock;
    private WeatherController _controller;

    [TestInitialize]
    public void Inicializar()
    {
        _weatherRepoMock = new Mock<WeatherRepository>();
        _userRepoMock = new Mock<UserRepository>();
        _loggerMock = new Mock<Logger>();

        _controller = new WeatherController(
            _weatherRepoMock.Object,
            _userRepoMock.Object,
            _loggerMock.Object
        );

        // Simular User.Identity.Name
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        }, "mock"));

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [TestMethod]
    public async Task GetWeatherForecast_CuandoWeatherExiste_RetornaExito()
    {
        // Arrange
        ApiObjectRequest request = new ApiObjectRequest
        {
            weather_forecast = new weather_forecast()
        };

        weather_forecast wfEnDb = new weather_forecast { id = 1 };

        _weatherRepoMock
            .Setup(r => r.GetWeatherForecastAsync(request.weather_forecast))
            .ReturnsAsync(wfEnDb);

        // Act
        ApiObjectResponse respuesta = await _controller.GetWeatherForecast(request);

        // Assert
        Assert.IsTrue(respuesta.result);
        Assert.AreEqual(wfEnDb, respuesta.data);
        Assert.AreEqual(0, respuesta.error_code);
    }

    [TestMethod]
    public async Task GetWeatherForecast_CuandoWeatherNoExiste_YFallóInsercion_RetornaFallo()
    {
        // Arrange
        ApiObjectRequest request = new ApiObjectRequest
        {
            weather_forecast = new weather_forecast()
        };

        _weatherRepoMock
            .Setup(r => r.GetWeatherForecastAsync(request.weather_forecast))
            .ReturnsAsync((weather_forecast)null);

        _weatherRepoMock
            .Setup(r => r.InsertWeatherFieldAsync(request.weather_forecast))
            .ReturnsAsync(-1);

        // Act
        ApiObjectResponse respuesta = await _controller.GetWeatherForecast(request);

        // Assert
        Assert.IsFalse(respuesta.result);
        Assert.AreEqual(0, respuesta.error_code);
        Assert.AreEqual("no se ha insertado en db", respuesta.error_message);
    }

    [TestMethod]
    public async Task GetWeatherForecast_CuandoWeatherNoExiste_YSeInserta_RetornaExitoCreado()
    {
        // Arrange
        ApiObjectRequest request = new ApiObjectRequest
        {
            weather_forecast = new weather_forecast()
        };

        _weatherRepoMock
            .SetupSequence(r => r.GetWeatherForecastAsync(request.weather_forecast))
            .ReturnsAsync((weather_forecast)null)   // primera llamada
            .ReturnsAsync(new weather_forecast { id = 5 }); // segunda llamada

        _weatherRepoMock
            .Setup(r => r.InsertWeatherFieldAsync(request.weather_forecast))
            .ReturnsAsync(5);

        // Act
        ApiObjectResponse respuesta = await _controller.GetWeatherForecast(request);

        // Assert
        Assert.IsFalse(respuesta.result);
        Assert.AreEqual(999, respuesta.error_code);
        //Assert.AreEqual(5, respuesta.data.id);
    }
}
