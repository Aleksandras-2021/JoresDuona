using Microsoft.EntityFrameworkCore;
using PosAPI.Data;
using PosAPI.Data.DbContext;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PosShared.Models;
using PosAPI.Repositories;
using Microsoft.OpenApi.Models;
using PosAPI.Repositories;
using PosAPI.Repositories.Interfaces;
using PosAPI.Services;
using PosAPI.Services;
using PosAPI.Services.Interfaces;
using PosShared.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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

//Add repositories here
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITaxRepository, TaxRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

builder.Services.AddScoped<IOrderService, PosAPI.Services.OrderService>();
builder.Services.AddScoped<IItemService, PosAPI.Services.ItemService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IUserService, UserService>();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("https://localhost:5001") // Frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);

var jwtSettings = jwtSection.Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var cookie = context.Request.Cookies["authToken"];
            if (!string.IsNullOrEmpty(cookie))
            {
                context.Token = cookie;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401; // Unauthorized
            return Task.CompletedTask;
        }
    };

    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

//Remove this later, this is for auto creating admin account
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Ensure the database is created (and migrations are applied)
    context.Database.Migrate();

    // Check if there is at least one business in the Businesses table
    var business = context.Businesses.FirstOrDefault(b => b.Id == 1);

    if (business == null)
    {
        // If no business exists, create a default business
        business = new Business
        {
            Id = 1,
            Name = "Default Business",
            Address = "123 Business Ave",
            PhoneNumber = "+123",
            VATCode = "123",
            Email = "123@gmail.com",
            Type = BusinessType.Beauty
        };

        // Add the business to the database
        context.Businesses.Add(business);
        context.SaveChanges();
    }

    // Now that we have a business, we can safely create the admin user
    var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@gmail.com");

    if (adminUser == null)
    {
        // If no admin user exists, create a new one
        var newUser = new User
        {
            Id = 0, // This is typically auto-generated by the database
            Email = "admin@gmail.com",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"), // Hash the password
            BusinessId = business.Id, // Use the existing business's ID
            Name = "Admin User",
            Role = UserRole.SuperAdmin, // Adjust roles based on your implementation
            Phone = "1234567890", // Add any other details if necessary
            Address = "123 Admin Street", // Add any other details if necessary
            EmploymentStatus = EmploymentStatus.Active
        };

        context.Users.Add(newUser);
        context.SaveChanges();
    }
}



app.UseHttpsRedirection();

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
