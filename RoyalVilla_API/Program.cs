using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RoyalVilla_API.Data;
using RoyalVilla.Dto;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services;
using Scalar.AspNetCore;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using RoyalVilla_API.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtSettings:Secret").Value);

// Add services to the container.

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = false;
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
}).AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV";
    option.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(o =>
{
    o.CreateMap<Villa, VillaCreateDto>().ReverseMap();
    o.CreateMap<Villa, VillaUpdateDto>().ReverseMap();
    o.CreateMap<Villa, VillaDto>().ReverseMap();
    o.CreateMap<VillaUpdateDto, VillaDto>().ReverseMap();
    o.CreateMap<ApplicationUser, UserDto>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmenitiesCreateDto>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmenitiesUpdateDto>().ReverseMap();
    o.CreateMap<VillaAmenitiesUpdateDto, VillaAmenities>().ReverseMap();
    o.CreateMap<VillaAmenities, VillaAmenitiesDto>()
        .ForMember(des => des.VillaName, opt => opt.MapFrom(src => src.Villa != null ? src.Villa.Name : null));
    o.CreateMap<VillaAmenitiesDto, VillaAmenities>();
});

builder.Services.AddControllers();

var builderProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

foreach (var description in builderProvider.ApiVersionDescriptions)
{

    var versionName = description.GroupName;
    var versionNumber = description.ApiVersion.ToString();
    var displayName = $"Demo API -- {versionNumber}";

    builder.Services.AddOpenApi(versionName, options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Demo Royal API",
                Version = versionName,
                Description = displayName,
                Contact = new OpenApiContact
                {
                    Name = "Mohammed Adil",
                    Email = "mohammed.dev43@gmail.com"
                }
            };

            document.Components ??= new();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT Bearer token"
                }
            };

            document.Security = [
                new OpenApiSecurityRequirement{
                {new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
            }
            ];

            return Task.CompletedTask;
        });
    });
}


var app = builder.Build();

await SeedDataAsync(app);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
if (true)
{
    app.MapOpenApi("/openapi/{documentName}.json");

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.MapScalarApiReference(option =>
    {
        option.Title = "Demo Royal Villa API";

        var sortedVersion = provider.ApiVersionDescriptions.OrderBy(v => v.ApiVersion).ToList();

        foreach (var description in sortedVersion)
        {

            var versionName = description.GroupName;
            var versionNumber = description.ApiVersion.ToString();
            var displayName = $"Demo API -- {versionNumber}";

            var isDefault = description.ApiVersion.Equals(new ApiVersion(2, 0));

            option.AddDocument(versionName, displayName, $"/openapi/{versionName}.json", isDefault: true);
        }
    });
}

app.UseStaticFiles();

app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*"));

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.Run();


static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}