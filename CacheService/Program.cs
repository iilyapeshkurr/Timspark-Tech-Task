using CacheService.Interfaces;
using CacheService.Options;
using CacheService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("CacheSettings"));

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IIpCacheService, IpCacheService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
