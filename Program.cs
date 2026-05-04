using Microsoft.EntityFrameworkCore;
using ArgosApi.Data;
using ArgosApi.Handlers;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ClockDataProcessor>();
builder.Services.AddScoped<IAuthService, AuthService>(); // Add AuthService
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4173",
                "http://localhost:4174",
                "http://172.30.101.73:4173",
                "http://172.30.101.73:4174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configure Swagger to use JWT authorization
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingresa el token JWT en este formato: Bearer {tu_token_aqui}",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = "Bearer",
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
    c.EnableAnnotations(); // Enable Swagger annotations for better documentation
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForArgosApiThatNeedsToBeAtLeast32CharactersLong!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("view:attendances", policy => policy.RequireClaim("permission", "view:attendances"));
        options.AddPolicy("create:users", policy => policy.RequireClaim("permission", "create:users"));
        options.AddPolicy("view:all", policy => policy.RequireClaim("permission", "view:all"));
        options.AddPolicy("delete:all", policy => policy.RequireClaim("permission", "delete:all"));
    }
);
builder.Services.AddHttpContextAccessor(); // Add HttpContextAccessor for accessing user information in services

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

// Use CORS policy
app.UseCors("AllowFrontend");

// Use Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Log de endpoints no definidos
app.Map("/{**catchAll}", async (HttpContext context) =>
{
    context.Request.EnableBuffering(); // 🔥 permite leer el body varias veces

    var request = context.Request;

    // 📦 BODY
    string body = "";
    using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
    {
        body = await reader.ReadToEndAsync();
        request.Body.Position = 0; // importante
    }

    // 📌 QUERY PARAMS
    var queryParams = string.Join(", ", request.Query.Select(q => $"{q.Key}={q.Value}"));

    // 📌 HEADERS
    var headers = string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}"));

    // 📌 FORM (por si manda x-www-form-urlencoded)
    string formData = "";
    if (request.HasFormContentType)
    {
        var form = await request.ReadFormAsync();
        formData = string.Join(", ", form.Select(f => $"{f.Key}={f.Value}"));
    }

    // 📌 INFO GENERAL
    var log = $@"
---- REQUEST ----
DATE: {DateTime.Now}
IP: {context.Connection.RemoteIpAddress}
METHOD: {request.Method}
PATH: {request.Path}
QUERY: {queryParams}
HEADERS: {headers}
CONTENT-TYPE: {request.ContentType}
BODY: {body}
FORM: {formData}
-----------------

";

    await File.AppendAllTextAsync("logs.txt", log);

    // 🔥 RESPUESTA PARA ZKTeco
    if (request.Path.Value.Contains("/iclock"))
    {
        return Results.Text("OK");
    }
    return Results.Ok();
});

app.MapControllers();

app.Run();
