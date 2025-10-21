using BatchProcessorService.Interfaces;
using BatchProcessorService.Services;
using BatchProcessorService.HttpClientWrappers;
using Serilog;
using BatchProcessorService.Middlewares;
using FluentValidation;
using BatchProcessorService.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddSingleton<IIpBatchService, IpBatchService>();

var lookupServiceBaseUrl = builder.Configuration["ServiceUrls:LookupServiceBaseUrl"]
    ?? throw new InvalidOperationException("LookupServiceBaseUrl configuration is missing.");

builder.Services.AddHttpClient<IIpLookupHttpClient, IpLookupHttpClient>(client =>
{
    client.BaseAddress = new Uri(lookupServiceBaseUrl);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddSingleton<IValidator<IEnumerable<string>>, BatchProcessRequestValidator>();

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
