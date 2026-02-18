using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SqlServer;
using weatherapp.Data;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using weatherapp.Mappers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using weatherapp.Middleware;
using weatherapp.Requests;
using weatherapp.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Entity Framework Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddControllers(options =>
{
	options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddJsonOptions(options =>
{
	options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontendOrigin",
		policy => policy.WithOrigins("http://localhost:3000")
			.AllowAnyHeader()
			.AllowAnyMethod());
});

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Register Location Service
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ITrackLocationService, TrackLocationService>();
builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
builder.Services.AddScoped<IOpenWeatherService, OpenWeatherService>();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<ISyncScheduleService, SyncScheduleService>();
builder.Services.AddScoped<IGlobalSyncService, GlobalSyncService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpClient<IOpenWeatherMapHttpClient, OpenWeatherMapHttpClient>(client =>
{
	client.BaseAddress = new Uri("https://api.openweathermap.org/data/3.0/onecall?");
	client.Timeout = TimeSpan.FromSeconds(30);

	client.DefaultRequestHeaders.Accept.Add(
		new MediaTypeWithQualityHeaderValue("application/json"));
});


// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();


var app = builder.Build();

// Initialize global sync recurring job and user sync schedules at startup
using (var scope = app.Services.CreateScope())
{
	var globalSyncService = scope.ServiceProvider.GetRequiredService<IGlobalSyncService>();
	await globalSyncService.InitializeGlobalSyncAsync();

	var syncScheduleService = scope.ServiceProvider.GetRequiredService<ISyncScheduleService>();
	await syncScheduleService.InitializeAllUserSyncSchedulesAsync();
}

app.Use(async (context, next) =>
{
	var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
	var requestPath = context.Request.Path;
	var requestMethod = context.Request.Method;

	logger.LogInformation("Incoming request: {Method} {Path}", requestMethod, requestPath);

	await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontendOrigin");
app.UseRouting();
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add Hangfire middleware
app.UseHangfireDashboard();

app.MapControllers();
app.Run();