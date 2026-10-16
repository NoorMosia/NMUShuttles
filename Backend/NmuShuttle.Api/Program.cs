using System.Text;
using System.Threading.RateLimiting;
using FirebaseAdmin;
using FluentValidation;
using FluentValidation.AspNetCore;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NmuShuttle.Api.BackgroundServices;
using NmuShuttle.Api.Data;
using NmuShuttle.Api.Infrastructure;
using NmuShuttle.Api.Services;

// The app stores all DateTime values as UTC (Kind=Utc) and maps them to
// PostgreSQL `timestamp without time zone` columns. Npgsql 6+ otherwise rejects
// writing Utc-kind DateTimes to `timestamp`. This switch restores the legacy
// behaviour so our UTC-in/UTC-out semantics are preserved unchanged.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────────
// Database
// ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ShuttleDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(3)));

// ──────────────────────────────────────────────────────────────────
// Authentication — JWT Bearer
// ──────────────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("MarshalOnly", policy => policy.RequireRole("Marshal"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ──────────────────────────────────────────────────────────────────
// JWT Token Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ──────────────────────────────────────────────────────────────────
// Email Service — Resend (transactional email for OTP delivery)
// ──────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<IEmailService, ResendEmailService>();

// ──────────────────────────────────────────────────────────────────
// OTP Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IOtpService, OtpService>();

// ──────────────────────────────────────────────────────────────────
// Trip State Machine Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITripStateMachineService, TripStateMachineService>();

// ──────────────────────────────────────────────────────────────────
// Capacity & Convoy Ordering Services
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICapacityService, CapacityService>();
builder.Services.AddScoped<IConvoyOrderingService, ConvoyOrderingService>();
builder.Services.AddScoped<IStopDefaultConvoyService, StopDefaultConvoyService>();
builder.Services.AddScoped<IScheduleTimeCampusService, ScheduleTimeCampusService>();
builder.Services.AddScoped<ITripNoteService, TripNoteService>();

// ──────────────────────────────────────────────────────────────────
// Notification Services
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IFcmSender, FcmNotificationSender>();

// ──────────────────────────────────────────────────────────────────
// Firebase initialization (optional — graceful if not configured)
// ──────────────────────────────────────────────────────────────────
// Accept either the config-section form (Firebase:ServiceAccountPath) or the
// documented environment variable (FCM_SERVICE_ACCOUNT_PATH). The first
// non-empty value wins, so existing deployments keep working.
var fcmPath = builder.Configuration["Firebase:ServiceAccountPath"];
if (string.IsNullOrEmpty(fcmPath))
{
    fcmPath = builder.Configuration["FCM_SERVICE_ACCOUNT_PATH"];
}
if (!string.IsNullOrEmpty(fcmPath) && File.Exists(fcmPath))
{
    FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile(fcmPath) });
}
else
{
    Console.WriteLine("⚠️  Firebase service account not configured. FCM push notifications disabled.");
}

// ──────────────────────────────────────────────────────────────────
// CSV User Import Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICsvUserImportService, CsvUserImportService>();

// ──────────────────────────────────────────────────────────────────
// Booking Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IBookingService, BookingService>();

// ──────────────────────────────────────────────────────────────────
// Check-In Service
// ──────────────────────────────────────────────────────────────────
builder.Services.Configure<ScanWindowOptions>(
    builder.Configuration.GetSection(ScanWindowOptions.SectionName));
builder.Services.AddScoped<ICheckInService, CheckInService>();

// ──────────────────────────────────────────────────────────────────
// Breakdown Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IBreakdownService, BreakdownService>();

// ──────────────────────────────────────────────────────────────────
// Vendor Analytics Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IVendorAnalyticsService, VendorAnalyticsService>();

// ──────────────────────────────────────────────────────────────────
// CORS — restricted to known Flutter Web origin
// ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, allow any localhost origin (Flutter Web uses random ports)
            // plus any explicitly configured origins (e.g. the GitHub Pages demo frontend
            // reached over an ngrok tunnel).
            var devAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();
            policy.SetIsOriginAllowed(origin =>
                    new Uri(origin).Host == "localhost" || devAllowedOrigins.Contains(origin))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(
                    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:8080" })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// ──────────────────────────────────────────────────────────────────
// Rate Limiting — 5 requests/min per IP on OTP endpoints
// ──────────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("OtpRateLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// ──────────────────────────────────────────────────────────────────
// FluentValidation
// ──────────────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// ──────────────────────────────────────────────────────────────────
// Swagger / OpenAPI
// ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NMU Shuttle API",
        Version = "v1",
        Description = "NMU Shuttle Management Network API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// ──────────────────────────────────────────────────────────────────
// Waitlist Promotion Service
// ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IWaitlistPromotionService, WaitlistPromotionService>();

// ──────────────────────────────────────────────────────────────────
// Background Services (disabled temporarily for local dev — re-enable for production)
// ──────────────────────────────────────────────────────────────────
builder.Services.AddHostedService<RollingInjectionService>();
builder.Services.AddHostedService<LockoutGateService>();

// ──────────────────────────────────────────────────────────────────
// Controllers
// ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Emit every DateTime as a UTC instant with a trailing "Z" so clients
        // (e.g. Dart's DateTime.parse) don't misread stored-UTC values as local time.
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableUtcDateTimeJsonConverter());
    });

// ──────────────────────────────────────────────────────────────────
// ProblemDetails exception handler
// ──────────────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ──────────────────────────────────────────────────────────────────
// Build App
// ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ──────────────────────────────────────────────────────────────────
// Apply EF Core migrations at startup
// ──────────────────────────────────────────────────────────────────
// The deployed database (Supabase) starts empty, and nothing else in this
// app creates the schema. Running Migrate() here guarantees all tables,
// constraints, indexes and HasData seed rows exist before the seeders and
// StartupTripGenerator below run against them.
using (var migrateScope = app.Services.CreateScope())
{
    var db = migrateScope.ServiceProvider.GetRequiredService<ShuttleDbContext>();
    await db.Database.MigrateAsync();
}

// ──────────────────────────────────────────────────────────────────
// Middleware Pipeline
// ──────────────────────────────────────────────────────────────────

// ProblemDetails exception handling (must be first)
app.UseExceptionHandler();
app.UseStatusCodePages();

// Swagger — available in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NMU Shuttle API v1");
    });
}

app.UseCors("AllowFlutterWeb");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<NmuShuttle.Api.Infrastructure.ActiveUserMiddleware>();

app.MapControllers();

// Seed historical data in development (only runs if TripInstances table is empty)
if (app.Environment.IsDevelopment())
{
    using var seedScope = app.Services.CreateScope();
    var seeder = new NmuShuttle.Api.Data.HistoricalDataSeeder(
        seedScope.ServiceProvider.GetRequiredService<ShuttleDbContext>());
    await seeder.SeedAsync();
}

// Generate trips on startup (today through today+3) so there's data immediately
await NmuShuttle.Api.Infrastructure.StartupTripGenerator.GenerateTripsAsync(app.Services);

// Seed a demo trip at capacity with waitlisted students (for notification demo)
if (app.Environment.IsDevelopment())
{
    using var demoScope = app.Services.CreateScope();
    var demoSeeder = new NmuShuttle.Api.Data.DemoWaitlistSeeder(
        demoScope.ServiceProvider.GetRequiredService<NmuShuttle.Api.Data.ShuttleDbContext>(),
        demoScope.ServiceProvider.GetRequiredService<NmuShuttle.Api.Services.IWaitlistPromotionService>(),
        demoScope.ServiceProvider.GetRequiredService<ILogger<NmuShuttle.Api.Data.DemoWaitlistSeeder>>());
    await demoSeeder.SeedAsync();
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
