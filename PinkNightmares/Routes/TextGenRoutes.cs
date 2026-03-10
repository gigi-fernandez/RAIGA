using Google.GenAI;
using PinkNightmares.Repositories;
using PinkNightmares.Services;

namespace PinkNightmares.Routes;

public class TextGenRoutes
{
    private static readonly string Prefix = "/gen/text";
    private static Client? _googleClient;
    
    public static void Map(WebApplication app)
    {
        var apiKey = app.Configuration["Google:ApiKey"];
        _googleClient = new Client(apiKey: apiKey);

        app.MapPost($"{Prefix}/test", async (HttpContext context, PinkDb db, TextRequestDTO req) =>
            {
                var user = UserService.GetCurrentUser(context, db);
                var currUser = user.Result;
                if (currUser == null) return Results.Unauthorized();

                currUser.CreditCount -= req.Cost;
                await db.SaveChangesAsync();

                string AiResponse = GenerateText(req.Prompt).Result;
                if (AiResponse is not string) return Results.InternalServerError();
                return Results.Content(AiResponse);
            })
            .Produces(200, contentType: "image/png")
            .Produces(404);
    }

    public static async Task<string> GenerateText(string prompt) {
        var response = await _googleClient.Models.GenerateContentAsync(
            model: "gemini-3.1-flash-lite-preview", contents: prompt
        );
        return response.Candidates[0].Content.Parts[0].Text;
    }

    
    private record TextRequestDTO(int Cost, string Prompt);
}