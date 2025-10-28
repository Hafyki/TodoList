using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TodoList.Data;
using TodoList.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//DataBase Connection
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Routing Config
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

//CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS policy", policy =>
    {

        policy.WithOrigins("http://localhost:8080", "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<ISyncService>();
builder.Services.AddScoped<IToDoService, ToDoService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}
app.UseCors("CORS policy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
