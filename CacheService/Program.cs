using CacheService.Interfaces;
using CacheService.Options;
using CacheService.Services;
using CacheService.Middlewares;
using Serilog;
using FluentValidation.AspNetCore;
using FluentValidation;
using Shared.Validators;
using IpLookupService.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("CacheSettings"));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IpDetailsValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<IpAddressValidator>();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IIpCacheService, IpCacheService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
