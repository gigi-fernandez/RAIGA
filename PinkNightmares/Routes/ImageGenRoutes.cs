using PinkNightmares.Repositories;
using PinkNightmares.Services;

namespace PinkNightmares.Routes;

public class ImageGenRoutes
{
    private static readonly string Prefix = "/gen/image";

    public static void Map(WebApplication app)
    {
        app.MapGet($"{Prefix}/example", () =>
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Routes", "example.png");

            if (!File.Exists(filePath))
            {
                return Results.NotFound("Image not found");
            }

            var fileBytes = File.ReadAllBytes(filePath);
            return Results.File(fileBytes, "image/png");
        })
        .Produces(200, contentType: "image/png")
        .Produces(404);

        app.MapPost($"{Prefix}/test", async (HttpContext context, PinkDb db, ImageRequestDTO req) =>
        {
            var user = UserService.GetCurrentUser(context, db);
            var currUser = user.Result;
            if (currUser == null) return Results.Unauthorized();

            currUser.CreditCount -= req.Cost;
            await db.SaveChangesAsync();
            return Results.Created($"Created image. Current credits", new {currUser.CreditCount});
        })
        .Produces(200, contentType: "image/png")
        .Produces(404);
    }

    private record ImageRequestDTO(int Cost);
}