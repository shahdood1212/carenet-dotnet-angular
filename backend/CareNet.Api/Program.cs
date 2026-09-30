using System.Text;
using CareNet.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var b = WebApplication.CreateBuilder(args);
b.Services.AddDbContext<AppDb>(o => o.UseSqlServer(b.Configuration.GetConnectionString("Default")));
b.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
b.Services.AddControllers();
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
        Array.Empty<string>() }});
});
b.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, ValidateAudience = false, ValidateLifetime = true,
        RoleClaimType = "role", NameClaimType = "sub",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(b.Configuration["Jwt:Key"]!))
    };
});
b.Services.AddAuthorization();
b.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = b.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
    if (!db.Users.Any(u => u.Role == Role.Admin))
    {
        var admin = new User { Name = "Admin", Email = "admin@carenet.com", Role = Role.Admin };
        admin.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>().HashPassword(admin, "Admin@123");
        db.Users.Add(admin);
        db.SaveChanges();
    }
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
