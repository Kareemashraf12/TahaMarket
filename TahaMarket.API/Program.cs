using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using TahaMarket.Application.Services;
using TahaMarket.Application.Services.Common;
using TahaMarket.Infrastructure.Data;
using TahaMarket.Infrastructure.Hubs;


var builder = WebApplication.CreateBuilder(args);

// ------------------- Services -------------------

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<RatingService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FileUrlService>();
builder.Services.AddScoped<DeliveryService>();
builder.Services.AddScoped<FileUrlService>();
builder.Services.AddScoped<DistanceService>();
builder.Services.AddScoped<DeliveryPricingService>();
builder.Services.AddScoped<UserAddressService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<StoreSectionService>();
builder.Services.AddScoped<OfferService>();
builder.Services.AddMemoryCache();



builder.Configuration.AddJsonFile("appsettings.json", optional: false);

//  cycle error
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

// ------------------- JWT ------------------

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        ),

        ClockSkew = TimeSpan.Zero
    };
});

// SignalR
builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer YOUR_TOKEN"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ------------------- Middleware -------------------

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/hubs/orders");

// Localization
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

// ------------------- Database -------------------

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    dbContext.Database.Migrate();


    SeedData.EnsureAdminExists(dbContext);
}
//app.MapGet("/", () => "Server is working ");

app.Run();