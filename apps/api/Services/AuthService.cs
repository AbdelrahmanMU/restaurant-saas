using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Auth;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class AuthService(AppDbContext db, ITokenService tokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var roles = user.UserRoles.Select(r => r.Role).ToArray();

        var token = tokenService.GenerateToken(
            user.Id, user.PhoneNumber, roles, user.FullName,
            user.BranchId, user.RestaurantId);

        return new LoginResponse(
            token,
            roles.Select(r => r.ToString()).ToArray(),
            user.FullName,
            user.BranchId,
            user.RestaurantId);
    }

    public async Task<(LoginResponse? Result, string? Error)> RegisterOwnerAsync(RegisterOwnerRequest request)
    {
        if (await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
            return (null, "PHONE_TAKEN");

        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = request.RestaurantName,
            Slug = GenerateSlug(request.RestaurantName)
        };

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurant.Id,
            Name = request.BranchName ?? "الفرع الرئيسي",
            Address = string.Empty
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            RestaurantId = restaurant.Id,
            BranchId = branch.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = Role.Owner,
            RestaurantId = restaurant.Id,
            BranchId = branch.Id,
            AssignedAt = DateTime.UtcNow
        };

        db.Restaurants.Add(restaurant);
        db.Branches.Add(branch);
        db.Users.Add(user);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        var roles = new[] { Role.Owner };

        var token = tokenService.GenerateToken(
            user.Id, user.PhoneNumber, roles, user.FullName,
            user.BranchId, user.RestaurantId);

        return (new LoginResponse(
            token,
            roles.Select(r => r.ToString()).ToArray(),
            user.FullName,
            user.BranchId,
            user.RestaurantId), null);
    }

    public async Task<LoginResponse?> ActivateInviteAsync(ActivateInviteRequest request)
    {
        if (!Guid.TryParse(request.InviteToken, out var inviteId))
            return null;

        var invite = await db.UserInvites.FirstOrDefaultAsync(i =>
            i.Id == inviteId &&
            !i.IsAccepted &&
            i.ExpiresAt > DateTime.UtcNow);

        if (invite is null) return null;
        if (invite.PhoneNumber != request.PhoneNumber) return null;

        if (await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = invite.PhoneNumber,
            FullName = request.FullName,
            BranchId = invite.BranchId,
            RestaurantId = invite.RestaurantId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = invite.Role,
            RestaurantId = invite.RestaurantId,
            BranchId = invite.BranchId,
            AssignedAt = DateTime.UtcNow
        };

        invite.IsAccepted = true;
        db.Users.Add(user);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        var roles = new[] { invite.Role };

        var token = tokenService.GenerateToken(
            user.Id, user.PhoneNumber, roles, user.FullName,
            user.BranchId, user.RestaurantId);

        return new LoginResponse(
            token,
            roles.Select(r => r.ToString()).ToArray(),
            user.FullName,
            user.BranchId,
            user.RestaurantId);
    }

    private static string GenerateSlug(string name)
    {
        var slug = Regex.Replace(name, @"[^a-zA-Z0-9]", "-").Trim('-').ToLower();
        if (string.IsNullOrWhiteSpace(slug)) slug = "restaurant";
        return $"{slug}-{Guid.NewGuid():N}"[..Math.Min(32, slug.Length + 15)];
    }
}
