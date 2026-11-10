#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthControllerTests
{
    private static (AuthController controller, RaceDayProg6212Context db) Create()
    {
        var options = new DbContextOptionsBuilder<RaceDayProg6212Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new RaceDayProg6212Context(options);
        var controller = new AuthController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            }
        };
        return (controller, db);
    }

    private static RegisterDto NewUser(string email = "runner@test.com")
    {
        return new()
        {
            FirstName = "Test",
            LastName = "Runner",
            Email = email,
            Password = "Password123"
        };
    }

    [Fact]
    public async Task Register_StoresHashedPassword_AndRunnerRole()
    {
        var (controller, db) = Create();

        var result = await controller.Register(NewUser());

        var created = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);

        var saved = await db.Users.SingleAsync();
        Assert.Equal("Runner", saved.Role);
        Assert.NotEqual("Password123", saved.PasswordHash);
        Assert.StartsWith("$2", saved.PasswordHash);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var (controller, _) = Create();
        await controller.Register(NewUser());

        var result = await controller.Register(NewUser());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk_AndSetsSession()
    {
        var (controller, db) = Create();
        await controller.Register(NewUser());

        var result = await controller.Login(new LoginDto { Email = "runner@test.com", Password = "Password123" });

        Assert.IsType<OkObjectResult>(result.Result);
        var saved = await db.Users.SingleAsync();
        Assert.Equal(saved.UserId, controller.HttpContext.Session.GetInt32("UserId"));
        Assert.Equal("Runner", controller.HttpContext.Session.GetString("Role"));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized_AndNoSession()
    {
        var (controller, _) = Create();
        await controller.Register(NewUser());

        var result = await controller.Login(new LoginDto { Email = "runner@test.com", Password = "WrongPass1" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Null(controller.HttpContext.Session.GetInt32("UserId"));
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        var (controller, _) = Create();
        await controller.Register(NewUser());
        await controller.Login(new LoginDto { Email = "runner@test.com", Password = "Password123" });

        var result = controller.Logout();

        Assert.IsType<NoContentResult>(result);
        Assert.Null(controller.HttpContext.Session.GetInt32("UserId"));
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = new();

        public bool IsAvailable => true;
        public string Id => "test-session";
        public IEnumerable<string> Keys => _values.Keys;

        public void Clear()
        {
            _values.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _values.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _values[key] = value;
        }

        public bool TryGetValue(
            string key,
            [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out byte[] value)
        {
            if (_values.TryGetValue(key, out var stored))
            {
                value = stored;
                return true;
            }

            value = null!;
            return false;
        }
    }
}