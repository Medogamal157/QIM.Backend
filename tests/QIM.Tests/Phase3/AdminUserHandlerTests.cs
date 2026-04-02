using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QIM.Application.Features.AdminUsers;
using QIM.Application.DTOs.Admin;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;

namespace QIM.Tests.Phase3;

[TestClass]
public class AdminUserHandlerTests : TestBase
{
    private UserManager<ApplicationUser> _userManager = null!;
    private RoleManager<IdentityRole> _roleManager = null!;

    [TestInitialize]
    public async Task SetUp()
    {
        _serviceProvider = _builder.Services.BuildServiceProvider();
        var context = GetDbContext();
        context.Database.EnsureCreated();

        _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Seed roles
        foreach (var role in new[] { "SuperAdmin", "Admin", "Client", "Provider" })
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    [TestMethod]
    public async Task CreateAdminUser_ReturnsSuccess()
    {
        var handler = new CreateAdminUserHandler(_userManager);
        var result = await handler.Handle(
            new CreateAdminUserCommand(new CreateAdminUserRequest
            {
                FullName = "New Admin",
                Email = "newadmin@qim.com",
                Password = "Password123!",
                Role = "Admin"
            }), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("New Admin", result.Data!.FullName);
        Assert.IsTrue(result.Data.Roles.Contains("Admin"));
    }

    [TestMethod]
    public async Task CreateAdminUser_DuplicateEmail_ReturnsFailure()
    {
        var handler = new CreateAdminUserHandler(_userManager);
        await handler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "A", Email = "dup@qim.com", Password = "Password123!", Role = "Admin"
        }), CancellationToken.None);

        var result = await handler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "B", Email = "dup@qim.com", Password = "Password123!", Role = "Admin"
        }), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task CreateAdminUser_InvalidRole_ReturnsFailure()
    {
        var handler = new CreateAdminUserHandler(_userManager);
        var result = await handler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "A", Email = "inv@qim.com", Password = "Password123!", Role = "InvalidRole"
        }), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GetAllAdminUsers_ReturnsAdminsAndSuperAdmins()
    {
        var createHandler = new CreateAdminUserHandler(_userManager);
        await createHandler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "Admin1", Email = "a1@qim.com", Password = "Password123!", Role = "Admin"
        }), CancellationToken.None);
        await createHandler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "Super1", Email = "s1@qim.com", Password = "Password123!", Role = "SuperAdmin"
        }), CancellationToken.None);

        var handler = new GetAllAdminUsersHandler(_userManager);
        var result = await handler.Handle(new GetAllAdminUsersQuery(), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.Count >= 2);
    }

    [TestMethod]
    public async Task UpdateAdminUser_ChangesFields()
    {
        var createHandler = new CreateAdminUserHandler(_userManager);
        var created = await createHandler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "Original", Email = "upd@qim.com", Password = "Password123!", Role = "Admin"
        }), CancellationToken.None);

        var handler = new UpdateAdminUserHandler(_userManager);
        var result = await handler.Handle(
            new UpdateAdminUserCommand(created.Data!.Id, new UpdateAdminUserRequest { FullName = "Updated" }),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Updated", result.Data!.FullName);
    }

    [TestMethod]
    public async Task DeleteAdminUser_DeactivatesUser()
    {
        var createHandler = new CreateAdminUserHandler(_userManager);
        var created = await createHandler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "ToDeactivate", Email = "deact@qim.com", Password = "Password123!", Role = "Admin"
        }), CancellationToken.None);

        var handler = new DeleteAdminUserHandler(_userManager);
        var result = await handler.Handle(new DeleteAdminUserCommand(created.Data!.Id), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);

        var user = await _userManager.FindByIdAsync(created.Data!.Id);
        Assert.IsFalse(user!.IsActive);
    }

    [TestMethod]
    public async Task ChangeAdminRole_ChangesRole()
    {
        var createHandler = new CreateAdminUserHandler(_userManager);
        var created = await createHandler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "RoleChange", Email = "role@qim.com", Password = "Password123!", Role = "Admin"
        }), CancellationToken.None);

        var handler = new ChangeAdminRoleHandler(_userManager);
        var result = await handler.Handle(
            new ChangeAdminRoleCommand(created.Data!.Id, new ChangeAdminRoleRequest { Role = "SuperAdmin" }),
            CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.Roles.Contains("SuperAdmin"));
        Assert.IsFalse(result.Data.Roles.Contains("Admin"));
    }

    [TestMethod]
    public async Task ChangeAdminRole_InvalidRole_ReturnsFailure()
    {
        var createHandler = new CreateAdminUserHandler(_userManager);
        var created = await createHandler.Handle(new CreateAdminUserCommand(new CreateAdminUserRequest
        {
            FullName = "A", Email = "invrole@qim.com", Password = "Password123!", Role = "Admin"
        }), CancellationToken.None);

        var handler = new ChangeAdminRoleHandler(_userManager);
        var result = await handler.Handle(
            new ChangeAdminRoleCommand(created.Data!.Id, new ChangeAdminRoleRequest { Role = "BadRole" }),
            CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
    }
}
