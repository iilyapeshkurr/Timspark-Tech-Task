using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using IpLookupService.Middlewares;
using IpLookupService.Validators;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Configuration.AddEnvironmentVariables();

builder.Host.ConfigureSerilog();
builder.Services.ConfigureIpStack(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IpLookupRequestValidator>();

builder.Services.AddHttpClient("CacheService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CacheServiceBaseUrl"]!);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp");
});

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
