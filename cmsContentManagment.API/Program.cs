using cmsContentManagment.Application.Interfaces;
using cmsContentManagment.Infrastructure.Persistance;
using cmsContentManagment.Infrastructure.Repositories;
using cmsContentManagment.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string? redisOptionsConfiguration = builder.Configuration["Redis:Connection"];
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);


builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    redisOptions.Configuration = redisOptionsConfiguration;
});



builder.Services.AddControllers();
builder.Services.AddScoped<IContentManagmentService, ContentManagmentService>();

var app = builder.Build();
app.UseMiddleware<ErrorHandlingMiddleware>();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
