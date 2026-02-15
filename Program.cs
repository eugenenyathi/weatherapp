using System.Net.Http.Headers;
using System.Reflection;
using weatherapp.Data;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using weatherapp.Mappers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using weatherapp.Requests;
using weatherapp.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Entity Framework Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Register Location Service
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ITrackLocationService, TrackLocationService>();
builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
builder.Services.AddScoped<IOpenWeatherService, OpenWeatherService>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpClient<IOpenWeatherMapHttpClient, OpenWeatherMapHttpClient>(client =>
{
	client.BaseAddress = new Uri("https://api.openweathermap.org/data/3.0/onecall?");
	client.Timeout = TimeSpan.FromSeconds(30);

	client.DefaultRequestHeaders.Accept.Add(
		new MediaTypeWithQualityHeaderValue("application/json"));
});

// Register WeatherForecast Service

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
