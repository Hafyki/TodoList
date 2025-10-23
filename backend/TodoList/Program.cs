using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TodoList.Data;
using TodoList.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Banco de Dados
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Adição do CORS para evitar problemas rodando a aplicação localmente
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS policy", policy =>
                      {
                          policy.WithOrigins("http://localhost:8080")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

//Services
builder.Services.AddScoped<ToDoService>();
builder.Services.AddScoped<SyncService>();

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
