using FluentValidation;
using Utilio.Common.Cache.Implementations;
using Utilio.Common.Cache.Interfaces;
using Utilio.Common.Logger.Implementations;
using Utilio.Common.Logger.Interfaces;
using Utilio.Provider.OpcinaNovoSarajevo.Api.Validators;
using Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper;
using Utilio.Provider.Common.DataContracts.Request;
using Microsoft.Extensions.Caching.Memory;
using Utilio.Common.Utilities;
using Utilio.Provider.OpcinaNovoSarajevo.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMvc();

builder.Services.AddMemoryCache();

// Logger
builder.Services.AddSingleton<ILoggerAdapter, NLogAdapter>();

// Cache
builder.Services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

// Business Layer
builder.Services.AddHttpClient<IProviderScrapper, ProviderScrapper>().SetHandlerLifetime(TimeSpan.FromMinutes(5));
// builder.Services.AddTransient<IProviderScrapper, ProviderScrapper>();

// Validators
builder.Services.AddScoped(typeof(IValidator<FetchDataRequest>), typeof(FetchDataRequestValidator));

//Initialize ConfigHelper class with ProviderConfiguration
ConfigHelper.Initialize(builder.Configuration.GetSection("ProviderConfiguration"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.Use(async (context, next) =>
 {
     context.Request.EnableBuffering();
     await next();
 });

app.Run();
