using System.Text.Json.Serialization;
using MyXProject.Web;
using MyXProject.Web.Entities;
using MyXProject.Web.Utils;
using Xams.Core;

var useAuth = false;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureXamsLogging();

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


// Enable routing for static NextJs site in wwwroot folder
app.Use(async (context, next) => await RoutingUtil.SetupRoutes(context, next, app));

app.UseStaticFiles();
// After authentication & authorization
app.AddXamsApi(options =>
{
    options.UseDashboard = true;
    options.RequireAuthorization = useAuth;
    options.GetUserId = useAuth ? UserUtil.GetUserId : null;
});

app.Run();