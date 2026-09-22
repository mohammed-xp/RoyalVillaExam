using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using RoyalVilla.Dto;
using RoyalVillaWeb;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(60);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

builder.Services.AddAutoMapper(o =>
{
    o.CreateMap<VillaDto, VillaCreateDto>().ReverseMap();
    o.CreateMap<VillaDto, VillaUpdateDto>().ReverseMap();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.LoginPath = "/auth/login";
        options.AccessDeniedPath = "/auth/accessdenied";
    });

SD.ApiBaseUrl = builder.Configuration.GetValue<string>("ServiceUrls:VillaApi");

builder.Services.AddHttpClient("RoyalVillaApi", client =>
{
    var villaApi = SD.ApiBaseUrl;
    client.BaseAddress = new Uri(villaApi);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<IVillaService, VillaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
