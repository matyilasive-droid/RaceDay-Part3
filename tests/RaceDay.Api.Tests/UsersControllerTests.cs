using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Api.Tests;

public class UsersControllerTests
{
    private RaceDayProg6212Context CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayProg6212Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayProg6212Context(options);
    }

    [Fact]
    public async Task PostUser_Should_Create_User_With_Hashed_Password()
    {
        using var ctx = CreateContext();
        var controller = new UsersController(ctx);

        var dto = new UserCreateDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Secret123!",
            Role = "Runner"
        };

        var result = await controller.PostUser(dto);

        var created = await ctx.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        created.Should().NotBeNull();
        created.PasswordHash.Should().NotBeNullOrWhiteSpace();
        BCrypt.Net.BCrypt.Verify(dto.Password, created.PasswordHash).Should().BeTrue();
    }
}
