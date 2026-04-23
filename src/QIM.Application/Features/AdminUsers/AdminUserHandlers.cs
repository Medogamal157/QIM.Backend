using MediatR;
using Microsoft.AspNetCore.Identity;
using QIM.Application.DTOs.Admin;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;
using QIM.Shared.Models;

namespace QIM.Application.Features.AdminUsers;

// ── Queries ──

public record GetAllAdminUsersQuery : IRequest<Result<List<AdminUserDto>>>;

public class GetAllAdminUsersHandler : IRequestHandler<GetAllAdminUsersQuery, Result<List<AdminUserDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllAdminUsersHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<Result<List<AdminUserDto>>> Handle(GetAllAdminUsersQuery request, CancellationToken ct)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");

        var allAdmins = admins.Union(superAdmins).DistinctBy(u => u.Id).ToList();

        var dtos = new List<AdminUserDto>();
        foreach (var user in allAdmins)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(new AdminUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            });
        }

        return Result<List<AdminUserDto>>.Success(dtos);
    }
}

// ── Commands ──

public record CreateAdminUserCommand(CreateAdminUserRequest Data) : IRequest<Result<AdminUserDto>>;

public class CreateAdminUserHandler : IRequestHandler<CreateAdminUserCommand, Result<AdminUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateAdminUserHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<Result<AdminUserDto>> Handle(CreateAdminUserCommand request, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(request.Data.Email);
        if (existing is not null)
            return Result<AdminUserDto>.Failure("A user with this email already exists.");

        var validRoles = new[] { "Admin", "SuperAdmin" };
        if (!validRoles.Contains(request.Data.Role))
            return Result<AdminUserDto>.Failure("Role must be Admin or SuperAdmin.");

        var user = new ApplicationUser
        {
            FullName = request.Data.FullName,
            Email = request.Data.Email,
            UserName = request.Data.Email,
            PhoneNumber = request.Data.PhoneNumber,
            UserType = UserType.Admin,
            IsActive = true,
            IsVerified = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Data.Password);
        if (!result.Succeeded)
            return Result<AdminUserDto>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, request.Data.Role);

        return Result<AdminUserDto>.Success(new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            Roles = new List<string> { request.Data.Role }
        });
    }
}

public record UpdateAdminUserCommand(string Id, UpdateAdminUserRequest Data) : IRequest<Result<AdminUserDto>>;

public class UpdateAdminUserHandler : IRequestHandler<UpdateAdminUserCommand, Result<AdminUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateAdminUserHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<Result<AdminUserDto>> Handle(UpdateAdminUserCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<AdminUserDto>.Failure($"Admin user with Id {request.Id} was not found.");

        if (request.Data.FullName is not null) user.FullName = request.Data.FullName;
        if (request.Data.Email is not null)
        {
            user.Email = request.Data.Email;
            user.UserName = request.Data.Email;
        }
        if (request.Data.PhoneNumber is not null) user.PhoneNumber = request.Data.PhoneNumber;
        if (request.Data.IsActive.HasValue) user.IsActive = request.Data.IsActive.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<AdminUserDto>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);

        return Result<AdminUserDto>.Success(new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            Roles = roles.ToList()
        });
    }
}

public record DeleteAdminUserCommand(string Id) : IRequest<Result>;

public class DeleteAdminUserHandler : IRequestHandler<DeleteAdminUserCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteAdminUserHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<Result> Handle(DeleteAdminUserCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result.Failure($"Admin user with Id {request.Id} was not found.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return Result.Success("Admin user deactivated.");
    }
}

public record ChangeAdminRoleCommand(string Id, ChangeAdminRoleRequest Data) : IRequest<Result<AdminUserDto>>;

public class ChangeAdminRoleHandler : IRequestHandler<ChangeAdminRoleCommand, Result<AdminUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangeAdminRoleHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<Result<AdminUserDto>> Handle(ChangeAdminRoleCommand request, CancellationToken ct)
    {
        var validRoles = new[] { "Admin", "SuperAdmin" };
        if (!validRoles.Contains(request.Data.Role))
            return Result<AdminUserDto>.Failure("Role must be Admin or SuperAdmin.");

        var user = await _userManager.FindByIdAsync(request.Id);
        if (user is null)
            return Result<AdminUserDto>.Failure($"Admin user with Id {request.Id} was not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, request.Data.Role);

        return Result<AdminUserDto>.Success(new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            Roles = new List<string> { request.Data.Role }
        });
    }
}
