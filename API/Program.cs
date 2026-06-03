using API.Application.Interfaces;
using API.Application.Services;
using API.Common.Mappings;
using API.Common.Middlewares;
using API.Common.Security;
using API.Data;
using API.Data.Interfaces;
using API.Data.Repositories;
using API.Services.Implementations;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// --------------------------------------------------------------------
// DATABASE CONTEXTS
// --------------------------------------------------------------------
// EF DbContext (ORM)
builder.Services.AddDbContext<EFDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Dapper context (raw SQL / Stored Procedure)
builder.Services.AddScoped<DapperDbContext>();

// --------------------------------------------------------------------
// REPOSITORIES & UNIT OF WORK
// --------------------------------------------------------------------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDapperRepository, DapperRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --------------------------------------------------------------------
// USER CONTEXT SERVICE
// --------------------------------------------------------------------
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

// --------------------------------------------------------------------
// APPLICATION SERVICES
// --------------------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// --------------------------------------------------------------------
// AUTOMAPPER, CONTROLLERS, MIDDLEWARE
// --------------------------------------------------------------------
// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Register CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:8081", "http://localhost:8082")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mini-POS API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] {} }
    });
});

// Jwt Configuration
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// --------------------------------------------------------------------
// MIDDLEWARE PIPELINE
// --------------------------------------------------------------------
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

// Cho phép load file t?nh c?a Angular t? wwwroot
app.UseStaticFiles();

app.UseMiddleware<ExceptionMiddleware>();

// Apply CORS
app.UseCors("AllowAngularClient");

app.UseAuthorization();

app.MapControllers();

// C?u hình Fallback ?? Angular x? lý Routing phía client
// Ý ngh?a: N?u request không kh?p v?i Controller nào ? trên, 
// thì tr? v? index.html ?? Angular t? x? lý Routing.
app.MapFallbackToFile("index.html");

app.Run();
