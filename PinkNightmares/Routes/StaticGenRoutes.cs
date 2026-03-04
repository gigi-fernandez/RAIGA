using PinkNightmares.Repositories;
using PinkNightmares.Services;

namespace PinkNightmares.Routes;

public class StaticGenRoutes
{
    private static readonly string Prefix = "/gen";

    public static void Map(WebApplication app)
    {
        app.MapPost($"{Prefix}/image", async (HttpContext context, PinkDb db, ImageRequestDTO req) =>
        {
            var user = UserService.GetCurrentUser(context, db);
            var currUser = user.Result;
            if (currUser == null) return Results.Unauthorized();

            currUser.CreditCount -= req.Cost;
            await db.SaveChangesAsync();
            return Results.Created($"Created image. Current credits", new {currUser.CreditCount});
        });
    }

    private record ImageRequestDTO(int Cost);
}