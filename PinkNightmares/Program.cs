using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PinkNightmares.Repositories;
using PinkNightmares.Routes;
using PinkNightmares.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<PinkDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<TokenGenerator>();

builder.Services.AddAuthentication().AddJwtBearer(jwtOptions =>
    {
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection("Jwt").GetValue<string>("Issuer"),
            ValidAudience = builder.Configuration.GetSection("Jwt").GetValue<string>("Audience"),
            IssuerSigningKey = new SymmetricSecurityKey
                (
                    Encoding.UTF8.GetBytes(
                        builder.Configuration.GetSection("Jwt").GetValue<string>("Key")!)
                )
        };
    });
builder.Services.AddAuthorization();

// Converts enum values to keys in API responses
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var app = builder.Build();

// Seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PinkDb>();
    var adminEmail = builder.Configuration["AdminUser:Email"] ?? "admin@pinknightmares.com";

    // Create admin if it doesn't exist
    if (!await db.Users.AnyAsync(u => u.Role == PinkNightmares.Models.UserRole.Admin))
    {
        var adminUser = new PinkNightmares.Models.User
        {
            Email = adminEmail,
            Name = "Admin",
            Role = PinkNightmares.Models.UserRole.Admin
        };
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();
        app.Logger.LogInformation($"Created admin user: {adminEmail}");
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapping app to endpoints
AuthRoutes.Map(app);
UserRoutes.Map(app);
ImageGenRoutes.Map(app);
TextGenRoutes.Map(app);

app.Run();