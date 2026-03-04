using System.Security.Claims;
using EmailValidation;
using Microsoft.EntityFrameworkCore;
using PinkNightmares.Models;
using PinkNightmares.Repositories;
using PinkNightmares.Services;

namespace PinkNightmares.Routes;

public static class AuthRoutes
{
    private static readonly string Prefix = "/auth";
    public static void Map(WebApplication app)
    {
        app.MapPost($"{Prefix}/register", async (RegisterDTO user, PinkDb context, TokenGenerator tokenGenerator) =>
        {
            // If email is invalid return bad result
            if (!EmailValidator.Validate(user.Email))
                return Results.BadRequest("Email is in invalid format.");

            // If email already in the system, disallow, redirect to login function.
            var existingUser = UserExists(user.Email, context);
            if (existingUser.Result.Equals(true)) return Results.Conflict("A user with that email already exists");

            var newUser = await RegisterUser(user, context, tokenGenerator);
            
            return TypedResults.Created($"/auth/users/{newUser.Id}", newUser);
        });
        
        app.MapPost($"{Prefix}/login", async (string email, PinkDb context, TokenGenerator tokenGenerator) =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return Results.NotFound("No user with that email exists.");

            var accessToken = GetAccessToken(user.Id, user.Email, user.Role.ToString(), tokenGenerator);
            var response = new LoginResponseDTO(accessToken);

            return Results.Ok(response);
        });
        
        // There is no point for a logout endpoint at this point. Access tokens expire on their own and that's enough.
        /*app.MapPost($"{Prefix}/logout", (int userId) =>
        {
            // Placeholder
            Results.Ok();
        });*/
    }

    private static string GetAccessToken(int userId, string email, string role, TokenGenerator tokenGenerator)
    {
        var accessToken = tokenGenerator.GenerateToken(userId, email, role);
        if (String.IsNullOrEmpty(accessToken)) return "";
        return accessToken;
    }

    private static async Task<bool> UserExists(string email, PinkDb context)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email) is not null;
    }
    
    private static async Task<RegisterResponseDTO> RegisterUser(RegisterDTO user, PinkDb context, TokenGenerator tokenGenerator)
    {
        var newUser = new User
        {
            Email = user.Email,
            Name = user.Name,
            Role = UserRole.Free
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        var accessToken = GetAccessToken(newUser.Id, newUser.Email, newUser.Role.ToString(), tokenGenerator);
        var response = new RegisterResponseDTO(newUser.Id, newUser.Email, newUser.Name, accessToken);

        return response;
    }

    private record LoginResponseDTO(string AccessToken);
    private record RegisterDTO(string Email, string Name);
    private record RegisterResponseDTO(int Id, string Email, string Name, string AccessToken);
    
}

