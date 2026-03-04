using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PinkNightmares.Migrations;
using PinkNightmares.Models;
using PinkNightmares.Repositories;
using PinkNightmares.Services;

namespace PinkNightmares.Routes;

public static class UserRoutes
{
    private static readonly string Prefix = "user";

    public static void Map(WebApplication app)
    {
        app.MapGet($"{Prefix}/{{userId}}", async (int userId, PinkDb db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(user1 => user1.Id == userId);
            if (user == null) return Results.NotFound("No user with that ID exists");
            
            return Results.Ok(MapUserDTO(user));
        })
        .RequireAuthorization(policy => policy.RequireRole("Member", "Admin"));
        
        app.MapGet($"{Prefix}/me", async (HttpContext context, PinkDb db) =>
        {
            var user = UserService.GetCurrentUser(context, db).Result;
            if (user == null) return Results.Unauthorized();

            return Results.Ok(MapUserDTO(user));
        }).RequireAuthorization();
        
        app.MapPatch($"{Prefix}/me", async (HttpContext context, UserPatchAuthDTO patchAuth, PinkDb db) =>
        {
            var user = UserService.GetCurrentUser(context, db).Result;
            if (user == null) return Results.Unauthorized();

            PatchUserAuth(user, patchAuth);
            await db.SaveChangesAsync();

            return Results.Ok(MapUserDTO(user));
        }).RequireAuthorization(policy => policy.RequireRole("Member", "Admin"));

        
        app.MapGet($"{Prefix}/all", async (PinkDb db) =>
        {
            var users = await db.Users.ToListAsync();
            List<UserDTO> mappedUsers = [];
            foreach (var user in users)
                mappedUsers.Add(MapUserDTO(user));
            
            return mappedUsers.Count > 0 ? Results.Ok(mappedUsers) : Results.NotFound("No users were found.");
        }).RequireAuthorization(policy => policy.RequireRole("Member", "Admin"));

        app.MapPatch($"{Prefix}/{{userId}}/role", async (int userId, UserRole newRole, PinkDb db) =>
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null)
                return Results.NotFound("User not found");

            user.Role = newRole;
            await db.SaveChangesAsync();

            return Results.Ok(MapUserDTO(user));
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        
        app.MapPatch($"{Prefix}/{{userId}}", async (int userId, UserPatchAuthDTO patchAuth, PinkDb db) =>
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) 
                return Results.NotFound("User not found");
            
            PatchUserAuth(user, patchAuth);
            await db.SaveChangesAsync();
            
            return Results.Ok(MapUserDTO(user));
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        
        app.MapDelete($"{Prefix}/{{userId}}", async (int userId, PinkDb db) =>
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) 
                return Results.NotFound("User not found");
            
            db.Remove(user);
            await db.SaveChangesAsync();
            
            return Results.Ok($"User with email {user.Email} has been removed successfully.");
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        
        app.MapPatch($"{Prefix}/DEBUG/awardCredits/{{userId}}", async (int userId, int creditCount, PinkDb db) =>
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) 
                return Results.NotFound("User not found");

            user.CreditCount += creditCount;
            await db.SaveChangesAsync();

            return Results.Ok();
        }).RequireAuthorization(policy=> policy.RequireRole("Admin"));
    }
    private record UserDTO(int Id, string Email, string Name, UserRole Role, int CreditCount);
    private static UserDTO MapUserDTO(User user)
    {
        return new UserDTO(user.Id, user.Email, user.Name, user.Role, user.CreditCount);
    }

    private record UserPatchAuthDTO(string? Email, string? Name, string? Password);

    private static void PatchUserAuth(User user, UserPatchAuthDTO patch)
    {
        if (patch.Email != null) user.Email = patch.Email;
        if (patch.Name != null) user.Name = patch.Name;
        if (patch.Password != null) user.Password = patch.Password;
    }
}

