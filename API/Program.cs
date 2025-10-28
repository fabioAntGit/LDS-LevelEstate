using API.APIs.Interfaces;
using API.APIs.Services;
using API.AutenticacaoAutorizacao.Interfaces;
using API.AutenticacaoAutorizacao.Services;
using API.ChatVisitasFeedback.Chat.Interfaces;
using API.ChatVisitasFeedback.Chat.Services;
using API.ChatVisitasFeedback.Chat.WebSockets;
using API.ChatVisitasFeedback.Feedbacks.Interfaces;
using API.ChatVisitasFeedback.Feedbacks.Services;
using API.ChatVisitasFeedback.Visitas.Interfaces;
using API.ChatVisitasFeedback.Visitas.Services;
using API.Data;
using API.FavoritosNotificacoes.Favoritos.Interfaces;
using API.FavoritosNotificacoes.Favoritos.Services;
using API.FavoritosNotificacoes.Notificacoes.Interfaces;
using API.FavoritosNotificacoes.Notificacoes.Services;
using API.FavoritosNotificacoes.Notificacoes.WebSockets;
using API.Identity;
using API.Imoveis.Interfaces;
using API.Imoveis.Services;
using API.Perfis.Agente.Interfaces;
using API.Perfis.Agente.Services;
using API.PesquisasFiltros.Filtros.Interfaces;
using API.PesquisasFiltros.Filtros.Services;
using API.PesquisasFiltros.Pesquisas.Interfaces;
using API.PesquisasFiltros.Pesquisas.Services;
using API.RankingsBadges.Badges.Interfaces;
using API.RankingsBadges.Badges.Services;
using API.RankingsBadges.Rankings.Interfaces;
using API.RankingsBadges.Rankings.Services;
using API.TaskScheduler.HangFire;
using API.TaskScheduler.HangFire.Jobs;
using DotNetEnv;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

Env.Load();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<LevelEstateContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["JwtSettings:Issuer"],
        ValidAudience = config["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization(options =>
{
    IdentityData.AddRolePolicies(options);
});

// Add services to the container.

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

builder.Services.AddScoped<ChatWebSocketHandler>();

builder.Services.AddHttpClient<IMapboxService, MapboxService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IPropertySearchService, PropertySearchService>();
builder.Services.AddScoped<IPropertyOrderService, PropertyOrderService>();

builder.Services.AddScoped<IFavoriteService, FavoriteService>();

builder.Services.AddScoped<IAgentService, AgentService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<INotificationHandler, NotificationWebSocketHandler>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IRankingService, RankingService>();

builder.Services.AddScoped<ConvertPointsJob>();
builder.Services.AddScoped<BadgeAssignmentJob>();
builder.Services.AddScoped<CreateRankingJob>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Hangfire configuration
builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:7104"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });

    options.AddPolicy("ProdCors", policy =>
    {
        policy.WithOrigins("", "")
            .WithHeaders("Content-Type", "Authorization")
            .WithMethods("GET", "POST", "PUT", "DELETE");
    });
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}
else
{
    app.UseCors("ProdCors");
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<LevelEstateContext>();
        db.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

// WebSocket route
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/ws/chat") && context.WebSockets.IsWebSocketRequest)
    {
        var handler = context.RequestServices.GetRequiredService<ChatWebSocketHandler>();
        await handler.InvokeAsync(context);
    }
    else if (context.Request.Path.StartsWithSegments("/ws/notifications") && context.WebSockets.IsWebSocketRequest)
    {
        var handler = context.RequestServices.GetRequiredService<INotificationHandler>();
        await handler.HandleConnectionAsync(context);
    }
    else
    {
        await next();
    }
});

app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "LevelEstate - Background Jobs"
});
JobScheduler.MonthlyJobs();


app.Run();


