using EWOMS_Application_CQRS.Commands;
using EWOMS_Application_CQRS.Commands.UpdateUser;
using EWOMS_Application_CQRS.Commands.UserPost;
using EWOMS_Application_CQRS.Queries.LockoutUser;
using EWOMS_Application_CQRS.Queries.LoginQueries;
using EWOMS_Application_CQRS.Queries.PasswordReset;
using EWOMS_Application_CQRS.Queries.RegisteredUser;
using EWOMS_Application_CQRS.Queries.Audit;
using EWOMS_ExternalClassLibrary_DTO.UserData_DTO;
using EWOMS_Application_CQRS.Commands.UserDeletion;
using EWOMS_Application_CQRS.Queries.DeletedData;
using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using EWOMS_ClassLibrary.EmailSending;
using EWOMS_ClassLibrary.JWTToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using EWOMS_ClassLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

//DB Connection

// DB Connection - Configured for PostgreSQL (Neon)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("EWOMS_ClassLibrary")));

//-----------//


//---------Dependency Injection----------//

builder.Services.AddScoped<MasterUser_LoginHandler>();
builder.Services.AddScoped<LoginAuthentication_Token>();
builder.Services.AddScoped<MasterUser_LockoutHandler>();
builder.Services.AddScoped<MasterUser_RegisterHandler>();
builder.Services.AddScoped<MasterUser_ForgotPwdHandler>();
builder.Services.AddScoped<MasterGetUserDetails_RoleHandler>();
builder.Services.AddScoped<MasterUser_GetLockoutHandler>();
builder.Services.AddScoped<MasterForgotpassprofilehandler>();
builder.Services.AddScoped<MasterUser_NewNotificationHandler>();
builder.Services.AddScoped<MasterUser_MarkNotificationsReadHandler>();
builder.Services.AddScoped<MasterUser_MarkSingleNotificationReadHandler>();
builder.Services.AddScoped<MasterAdminUpdateHandler>();
builder.Services.AddScoped<MasterManagerUpdateHandler>();
builder.Services.AddScoped<MasterUserUpdateHandler>();
builder.Services.AddScoped<MasterUserDeletePostHandler>();
builder.Services.AddScoped<MasterUser_DeleteFullDataHandler>();

builder.Services.AddScoped<MasterUserPostHandler>();
builder.Services.AddScoped<MasterUserPostGet_Handler>();
builder.Services.AddScoped<MasterUserArchivePost>();
builder.Services.AddScoped<MasterUserBlurPostHandler>();
builder.Services.AddScoped<TogglePostLikeHandler>();
builder.Services.AddScoped<ToggleSavePostHandler>();
builder.Services.AddScoped<GetSavedPostsHandler>();
builder.Services.AddScoped<GetDashboardStatsHandler>();
builder.Services.AddScoped<AddCommentHandler>();
builder.Services.AddScoped<GetCommentsHandler>();
builder.Services.AddScoped<MasterUserStoryHandler>();
builder.Services.AddScoped<SeedStoryHandler>();
builder.Services.AddScoped<GetDeletedUsersHandler>();
builder.Services.AddScoped<GetDeletedPostsHandler>();
builder.Services.AddScoped<GetAdminAuditLogsHandler>();
builder.Services.AddScoped<TogglePersonalFileLikeHandler>();

builder.Services.AddScoped<ForgotPasswordTokenGenerate>();
builder.Services.AddScoped<ForgotPassword_EmailSentHandler>();




// Interface intergrated DI

builder.Services.AddScoped<IEmailService, SendingEmail>();

//---------------------------------------//

builder.Services.AddIdentity<MasterUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();




//-----Attempt lockout-----//

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

//-------------------------//


//-----JWT Swagger Authorize-----//

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Nexus API", Version = "v1" });
    options.CustomSchemaIds(type => type.FullName);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter Token -> {Token}"
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
            new string[] { }
        }
    });
});

//-------------------------------//

//-----JWT Authentication-----//

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

    // Allow SignalR to receive JWT from query string (WebSocket connections can't set headers)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path.Value ?? "";

            if (!string.IsNullOrEmpty(accessToken) &&
                (context.HttpContext.Request.Path.StartsWithSegments("/chatHub") || 
                 path.Contains("/private-videos/video", StringComparison.OrdinalIgnoreCase) ||
                 path.Contains("/private-images/image", StringComparison.OrdinalIgnoreCase)))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

//----------------------------//


//builder.Services.AddAuthorization();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Adminaccess", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("Manageraccess", policy =>
        policy.RequireRole("Manager"));

    options.AddPolicy("Useraccess", policy =>
        policy.RequireRole("User"));

    options.AddPolicy("Administration", policy =>
        policy.RequireRole("Admin", "Manager"));

    options.AddPolicy("AllowAll", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddCors(options => {
    // Development CORS policy: allow any localhost origin (any port) and any method/header.
    // NOTE: Restrictive origins should be used in production.
    options.AddPolicy("AllowLocalhost", policy => {
        policy.SetIsOriginAllowed(origin => {
            try {
                var uri = new Uri(origin);
                return uri.Host == "localhost";
            } catch {
                return false;
            }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Required for JWT auth header
    });
});



// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// SignalR with CamelCase JSON serialization and increased message size for images
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024 * 100; // 100MB limit for base64 images
    options.StreamBufferCapacity = 1024 * 1024 * 100;
}).AddJsonProtocol(options => {
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddSingleton<UserConnectionManager>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try 
    {
        await context.Database.MigrateAsync();

        // Seed Roles
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "User", "Manager" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Expense Categories
        if (!context.ExpenseCategories.Any())
        {
            context.ExpenseCategories.AddRange(
                new ExpenseCategory { Name = "Materials" },
                new ExpenseCategory { Name = "Labor" },
                new ExpenseCategory { Name = "Equipment" },
                new ExpenseCategory { Name = "Permits" },
                new ExpenseCategory { Name = "Other" }
            );
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
        
        try {
            System.IO.File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "migration_error.txt"), 
                ex.ToString() + "\n\nConnection String: " + connectionString);
        } catch {}
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger ALWAYS so we can test on MonsterASP
app.UseDeveloperExceptionPage(); // Temporarily enabled for production debugging
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    // Local development settings
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseCors("AllowLocalhost");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapHub<EWOMS_CoreAPI.Hubber.ChatHub>("/chatHub");
        
app.Run();
