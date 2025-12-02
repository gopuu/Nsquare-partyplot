using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ERPAdmin;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
//using static ERPAdmin.Controllers.CategoryMasterController;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession();

// JSON serialization: preserve PascalCase property names
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // 👈 Prevents camelCase
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add services to the container.
//builder.Services.AddControllersWithViews();
//builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Register services
builder.Services.AddScoped<ImageUploadService>();
//ConnCLass
//builder.Services.AddSingleton<ConnClass>(); // Or use Scoped if needed
//excel upload
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP Api", Version = "v1" });

    // Configure JWT authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "ERP-API", // Replace with your issuer
            ValidAudience = "ERP-Mobile", // Replace with your audience
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ERPADMIN975312498fhjfjh@ksjf&fkjbf#")) // Replace with your secret key
        };
    });


//domain URL
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
//builder.Services.AddHttpClient<EwayService>();

//builder.Services.AddScoped<IEwayBillHandler, EwayBillHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();


app.UseSwagger();

// This middleware serves the Swagger documentation UI
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API V1");
});

// Read custom error settings from appsettings.json
var config = builder.Configuration;
bool enableCustomErrors = config.GetValue<bool>("CustomErrors:EnableCustomErrors");
if (enableCustomErrors)
{
    app.UseExceptionHandler("/Error/General"); // Handles all uncaught exceptions
    app.UseStatusCodePagesWithReExecute("/Error/{0}"); // Redirects 404, 500, etc.
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
