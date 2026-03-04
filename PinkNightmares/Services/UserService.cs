using System.Security.Claims;
using PinkNightmares.Models;
using PinkNightmares.Repositories;

namespace PinkNightmares.Services;

public class UserService
{
    public static async Task<User?> GetCurrentUser(HttpContext context, PinkDb db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return null;

        return await db.Users.FindAsync(int.Parse(userIdClaim.Value));
    }
}