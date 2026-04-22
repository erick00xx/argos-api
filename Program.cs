using Microsoft.EntityFrameworkCore;
using ArgosApi.Data;
using ArgosApi.Handlers;
using ArgosApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ClockDataProcessor>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
