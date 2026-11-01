using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SupportAssistant.API.Data;
using SupportAssistant.API.Services;
using System.Text;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Keep Microsoft's new native OpenAPI generator

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IAIService, GeminiAIService>();

builder.Services
    .AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Support Assistant API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Create the initial Support Engineer account
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    const string roleName = "SupportEngineer";

    if (!await roleManager.RoleExistsAsync(roleName))
    {
        var roleResult = await roleManager.CreateAsync(
            new IdentityRole(roleName));

        if (!roleResult.Succeeded)
        {
            throw new Exception("Could not create SupportEngineer role.");
        }
    }

    var email = builder.Configuration["SupportEngineer:Email"];
    var password = builder.Configuration["SupportEngineer:Password"];

    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
    {
        throw new Exception("Support Engineer credentials are missing.");
    }

    var engineer = await userManager.FindByEmailAsync(email);

    if (engineer == null)
    {
        engineer = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        var createResult = await userManager.CreateAsync(
            engineer, password);

        if (!createResult.Succeeded)
        {
            throw new Exception("Could not create Support Engineer account: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }

    if (!await userManager.IsInRoleAsync(engineer, roleName))
    {
        var roleResult = await userManager.AddToRoleAsync(
            engineer, roleName);

        if (!roleResult.Succeeded)
        {
            throw new Exception("Could not assign SupportEngineer role.");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AngularApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();