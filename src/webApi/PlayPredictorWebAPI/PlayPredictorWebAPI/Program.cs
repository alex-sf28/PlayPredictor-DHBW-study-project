using g_map_compare_backend.Data;
using g_map_compare_backend.Mapping;
using g_map_compare_backend.Middlewares;
using g_map_compare_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlayPredictorWebAPI.Services;
using PlayPredictorWebAPI.Services.External;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL anbinden
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<OAuthStateService>();
builder.Services.AddScoped<OAuthService>();
builder.Services.AddScoped<AnalysisService>();
builder.Services.AddScoped<AccountApplicationService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<FaceitService>();

builder.Services.AddHttpContextAccessor();

// AutoMapper registrieren
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register external http clients (Google, etc.)
builder.Services.AddExternalHttpClients();

builder.Services.AddSwaggerGen(c =>
{
    // Entfernt "Dto" aus den Schema-IDs (also den tats‰chlichen Namen im JSON)
    c.CustomSchemaIds(type =>
    {
        var name = type.Name;
        return name.EndsWith("Dto")
            ? name[..^3] // alles auﬂer "Dto"
            : name;
    });
    // Sicherheit mit JWT-Token einrichten
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddCors(options =>
  options.AddPolicy("AllowNext", p =>
    p.WithOrigins("http://localhost:3000") // oder deine Domain
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()
  )
);




var app = builder.Build();

app.UseCors("AllowNext");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Middlewares
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
