using EmailValidation;

namespace PinkNightmares.Routes;

public static class AuthRoutes
{
    private static string _prefix = "/auth";
    public static void Map(WebApplication app)
    {
        app.MapPost($"{_prefix}/register", (RegisterDTO dto) =>
        {
            // If email is invalid return bad result
            if (!EmailValidator.Validate(dto.Email)) 
                return Results.BadRequest("Email is in invalid format.");
            // If email already in the system, disallow, redirect to login function.
            
            // Placeholder
            return Results.Ok();
        });
        app.MapPost($"{_prefix}/login", (LoginDTO dto) =>
        {
            // Placeholder
            Console.WriteLine("Hello there!");
            Results.Ok();
        });
        app.MapPost($"{_prefix}/logout", (LogoutDTO dto) =>
        {
            // Placeholder
            Results.Ok();
        });
    }

    /*private static Task Login(string email)
    {
        
    };*/

    private record RegisterDTO(string Email, string Name, DateOnly DoB);
    private record LoginDTO(string Email);
    private record LogoutDTO(int Id);
    
}

