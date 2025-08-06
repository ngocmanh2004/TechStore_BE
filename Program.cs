using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using TechStore_BE.DataConnection;
using TechStore_BE.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLSaling"));
});

builder.Services.Configure<Email>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<EmailService>();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".TechStore.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",                   // Cho dev local
            "https://tech-store-23eda.web.app"         // Cho FE trên Firebase
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls(
    "http://localhost:5000",
    "https://localhost:7139"
);


var app = builder.Build();

app.UseCors("AllowOrigin");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.Run();
