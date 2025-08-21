using System.Text.Json.Serialization;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Project.Web;
using Project.Web.Entities;
using Project.Web.Utils;
using Xams.Core;

/*
 * Set useAuth to true to enable Firebase authentication and authorization.
 * Add your Firebase project keys to the keys folder. Name them your project-id.json
 */
var aspNetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var useAuth = false;
var firebaseProjectId = string.Empty;
switch (aspNetEnvironment)
{
    case "Development":
        useAuth = false; // Set to true to enable auth in development
        firebaseProjectId = "firebase-dev";
        break;
    case "Test":
        useAuth = true;
        firebaseProjectId = "firebase-test";
        break;
    case "Production":
        useAuth = true;
        firebaseProjectId = "firebase-prod";
        break;
}


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddXamsServices<DataContext, AppUser>();
builder.Services.AddDbContext<DataContext>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
    options.SerializerOptions.PropertyNamingPolicy = null;
});

string corsPolicy = "_myAllowOrigins";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(x => x.AddPolicy(corsPolicy, policyBuilder =>
    {
        // Need to provide the origin of the frontend app
        // for secure SignalR connections
        policyBuilder.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    }));
}

if (useAuth)
{
    builder.Services.AddAuthorization();

    // Connect to firebase app for auth
    FirebaseApp.Create(options: new AppOptions()
    {
        Credential = GoogleCredential.FromFile($"./keys/{firebaseProjectId}.json")
    });

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = firebaseProjectId,

                // Validate the token expiry
                ValidateLifetime = true,

                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero,
            };

            // Enable JWT authentication for SignalR
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/xams/hub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors(corsPolicy);
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

if (useAuth)
{
    app.UseAuthentication();
    app.UseAuthorization();
}


// Serve static Next.js files and handle client-side routing
// app.Use(async (context, next) => await RoutingUtil.SetupRoutes(context, next, app));

app.UseStaticFiles();
app.AddXamsApi(options =>
{
    options.UseDashboard = true;
    options.RequireAuthorization = useAuth;
    options.GetUserId = useAuth ? UserUtil.GetUserId : null;
});

app.Run();