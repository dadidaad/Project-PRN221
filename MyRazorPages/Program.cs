using MyRazorPages.Hubs;
using MyRazorPages.Models;
using System.Text.Json.Serialization;
using MyRazorPages.config;
using MyRazorPages.EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyRazorPages.Config;
using RazorPageEx2.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyRazorPages.Utils;

var builder = WebApplication.CreateBuilder(args);


// Bo sung kien truc cho ung dung web vao container cua web server
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
var connectionString = builder.Configuration.GetConnectionString("prn221db");
builder.Services.AddDbContext<PRN221DBContext>(options =>
    options.UseSqlServer(connectionString));
var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddIdentity<Account, Role>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
}).AddDefaultTokenProviders();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddTransient<IRoleStore<Role>, RoleStore>();
builder.Services.AddTransient<IUserStore<Account>, AccountStore>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
   opt.TokenLifespan = TimeSpan.FromMinutes(15));
builder.Services
    .AddSignalR(o => { o.EnableDetailedErrors = true; })
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.SaveToken = true;
    o.RequireHttpsMetadata = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RequireExpirationTime = false,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireEmployeeRole",
         policy => policy.RequireRole(UserRoles.Employee));
    options.AddPolicy("RequireCustomerRole",
         policy => policy.RequireRole(UserRoles.Customer));
});
var app = builder.Build();
app.UseSession();
app.Use(async (context, next) =>
{
    var JWToken = context.Session.GetString("JWToken");
    if (!string.IsNullOrEmpty(JWToken))
    {
        context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
    }
    await next();
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapRazorPages();
app.MapHub<ServerHub>("/hub");
app.Run();
