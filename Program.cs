using System.Text;
using ClinicAppointmentCRM.Configuration;
using ClinicAppointmentCRM.Data;
using ClinicAppointmentCRM.Middleware;
using ClinicAppointmentCRM.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// STEP 1: Add Services to the Container
// ==========================================
// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add MVC Controllers
builder.Services.AddControllersWithViews();

// Configure Database
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// ==========================================
// STEP 2: Configure Authentication
// ==========================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? string.Empty)),
        ClockSkew = TimeSpan.FromMinutes(30)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["jwt"];
            return Task.CompletedTask;
        }
    };

    // Handle authentication failures
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                // API: Return JSON error
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"error\":\"Unauthorized\",\"message\":\"Authentication required\"}");
            }
            else
            {
                // MVC: Redirect to login
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect(
                    $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return Task.CompletedTask;
            }
        },
        OnForbidden = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                // API: Return JSON error
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"error\":\"Forbidden\",\"message\":\"Access denied\"}");
            }
            else
            {
                // MVC: Redirect to access denied
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect(
                    $"/Account/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return Task.CompletedTask;
            }
        }
    };
});

// ==========================================
// STEP 3: Configure Authorization Policies
// ==========================================

builder.Services.AddAuthorization(options =>
{
    // Single role policies
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
    options.AddPolicy("ReceptionOnly", policy => policy.RequireRole("Reception"));

    // Multiple role policies
    options.AddPolicy("AdminOrDoctor", policy => policy.RequireRole("Admin", "Doctor"));
    options.AddPolicy("AdminOrReception", policy => policy.RequireRole("Admin", "Reception"));
    options.AddPolicy("DoctorOrReception", policy => policy.RequireRole("Doctor", "Reception"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Doctor", "Reception"));

});

// ==========================================
// STEP 4: Register Application Services
// ==========================================

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IReceptionService, ReceptionService>();

// ==========================================
// STEP 5: Configure Swagger/OpenAPI
// ==========================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define Swagger Document
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Clinic Appointment CRM API",
        Description = "RESTful API for managing clinic appointments, patients, doctors, and prescriptions",
        Contact = new OpenApiContact
        {
            Name = "ClinicCRM Support Team",
            Email = "support@cliniccrm.com",
            Url = new Uri("https://cliniccrm.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "Use under license",
            Url = new Uri("https://cliniccrm.com/license")
        }
    });

    // ✅ IMPORTANT: Only include API controllers (exclude MVC controllers)
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Only include endpoints that start with /api/
        return apiDesc.RelativePath?.StartsWith("api/") ?? false;
    });

    // ✅ Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme. 
                      
Enter 'Bearer' [space] and then your token in the text input below.

Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });

    // Group endpoints by controller
    options.TagActionsBy(api => new[]
    {
        api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Unknown"
    });

    // Enable annotations (optional but useful)
    options.EnableAnnotations();

    // Use method names as operation IDs
    options.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out var methodInfo)
            ? methodInfo.Name
            : null;
    });
});

// ==========================================
// STEP 6: Configure CORS (if needed)
// ==========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseJwtCookie();
app.UseSession();
// ==========================================
// STEP 7: Configure HTTP Request Pipeline
// ==========================================

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    // ✅ Enable Swagger in Development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic CRM API V1");
        options.RoutePrefix = "swagger"; // Access at: https://localhost:xxxx/swagger
        options.DocumentTitle = "Clinic CRM API Documentation";

        // UI Customization
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();

        // Enable "Try it out" by default
        options.EnableTryItOutByDefault();
    });
}
else
{
    // Production error handling
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

// ✅ IMPORTANT: Order matters!
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();