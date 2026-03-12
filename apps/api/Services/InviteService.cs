using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Data;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Enums;
using RestaurantSaas.Api.DTOs.Invites;
using RestaurantSaas.Api.Services.Interfaces;

namespace RestaurantSaas.Api.Services;

public class InviteService(AppDbContext db) : IInviteService
{
    public async Task<(CreateInviteResponse? Result, string? Error)> CreateInviteAsync(
        CreateInviteRequest request,
        Guid restaurantId,
        string baseUrl)
    {
        if (await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
            return (null, "PHONE_TAKEN");

        if (!Enum.TryParse<Role>(request.Role, out var role))
            return (null, "INVALID_ROLE");

        var expiry = DateTime.UtcNow.AddDays(7);

        var invite = new UserInvite
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            BranchId = request.BranchId,
            PhoneNumber = request.PhoneNumber,
            Role = role,
            IsAccepted = false,
            ExpiresAt = expiry,
            CreatedAt = DateTime.UtcNow
        };

        db.UserInvites.Add(invite);
        await db.SaveChangesAsync();

        var activationLink = $"{baseUrl}/activate?token={invite.Id}";

        return (new CreateInviteResponse(invite.Id, activationLink, expiry), null);
    }
}
