using weatherapp.Data;
using weatherapp.Services;
using weatherapp.Services.Interfaces;
using weatherapp.Mappers;
using FluentValidation;
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

// Register AutoMapper

// Register Location Service
builder.Services.AddScoped<ILocationService, LocationService>();

// Register Fluent Validators
builder.Services.AddValidatorsFromAssemblyContaining<LocationRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
