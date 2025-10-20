using BatchProcessorService.Interfaces;
using BatchProcessorService.Services;
using BatchProcessorService.HttpClientWrappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IIpBatchService, IpBatchService>();

builder.Services.AddHttpClient<IIpCacheHttpClient, IpCacheHttpClient>();

builder.Services.AddHttpClient<IIpLookupHttpClient, IpLookupHttpClient>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
