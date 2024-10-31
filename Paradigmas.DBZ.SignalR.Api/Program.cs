using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Paradigmas.DBZ.SignalR.Api.SignalRHub;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Configure Swagger to use JWT Bearer token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<DBZCharactersStream>();


builder.Services.AddCors();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:5173"; // URL of your local Identity Server
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://localhost:7014",

            ValidateAudience = true,
            ValidAudiences = new[] { "https://localhost:5001" },

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ForTheLoveOfGodStoreAndLoadThisSecurely")),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

app.UseCors(policy =>
    policy.WithOrigins("http://localhost:5173") // Specify the allowed origin
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials() // Allow credentials for authentication
);

app.UseHttpsRedirection();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Returns a random character and requires a valid JWT token
app.MapGet("/dbz/character", () =>
{
    string[] characters = new[] { "Goku", "Picoro", "Vegueta", "Krilin" };
    var character = Enumerable.Range(1, 1).Select(index =>
        new Character
        (
            characters[Random.Shared.Next(0, 4)],
            Random.Shared.Next(0, 100)
        ));
    return character;
})
.WithName("GetCharacters")
.WithOpenApi()
.RequireAuthorization(); // Require authorization for this endpoint

app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<DBZCharactersHub>("/hub/DBZCharactersHub");

app.Run();

internal record Character(string Name, int StrengthLevel);
