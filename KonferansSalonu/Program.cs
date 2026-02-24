using KonferansSalonu.Components;
using KonferansSalonu.Models;
using KonferansSalonu.Services;
using Microsoft.EntityFrameworkCore;
using CurrieTechnologies.Razor.SweetAlert2;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Database connection
builder.Services.AddDbContext<ConferencedbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.AddSweetAlert2();

builder.Services.AddScoped<IClientUiService, ClientUiService>();

builder.Services.AddScoped<ISectionService, SectionService>();

builder.Services.AddScoped<ISeatGroupService, ISeatGroup>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
