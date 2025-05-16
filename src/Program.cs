using FitnessCheck.Data;
using Microsoft.EntityFrameworkCore;
using FitnessCheck.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add In-Memory caching
builder.Services.AddSingleton<GenderMemoryCache>();
builder.Services.AddSingleton<CohortMemoryCache>();

// Add JWT authorization
builder.Services.AddAuthentication().AddJwtBearer();

// Establish db connection
var connectionString = builder.Configuration["ConnectionStrings:MariaDb"];
var serverVersion = new MySqlServerVersion(new Version(8, 0, 26));
builder.Services.AddDbContextPool<FitnessCheckDbContext>(options => options.UseMySql(connectionString, serverVersion));

// Registering services
builder.Services.AddAutoMapper(typeof(FitnessCheckMappingProfile));
builder.Services.AddScoped<IPointsCalculator, PointsCalculator>();
builder.Services.AddScoped<IGenderService, GoogleCloudFunctionsGenderService>();
builder.Services.AddScoped<ICohortService, GoogleCloudFunctionsCohortService>();

// Configuration / Options
builder.Services.Configure<MaxAllowedAttemptsOptions>(builder.Configuration.GetSection(MaxAllowedAttemptsOptions.ConfigurationPath));
builder.Services.Configure<GoogleCloudFunctionOptions>(builder.Configuration.GetSection(GoogleCloudFunctionOptions.ConfigurationPath));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
