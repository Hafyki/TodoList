using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TodoList.Data;
using TodoList.DTO;
using TodoList.Models.Entities;

namespace TodoList.Services;
public class SyncService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _dbContext;

    public SyncService(IHttpClientFactory httpClientFactory, ApplicationDbContext dbContext)
    {
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
    }

    public async Task SyncData(string url)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Falha ao obter dados ({response.StatusCode})");
        
        var json = await response.Content.ReadAsStringAsync();
        var toDosDto = JsonSerializer.Deserialize<List<ToDoDto>>(json);
        if (toDosDto == null || !toDosDto.Any())
            return;

        // abre conexão e inicia transação (mantém conexão aberta para ExecuteSqlRaw e SaveChanges)
        var dbConnection = _dbContext.Database.GetDbConnection();
        await dbConnection.OpenAsync();
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            //Remove todos os ToDos existentes para garantir a sobrescrita
            _dbContext.ToDos.RemoveRange(_dbContext.ToDos);
            await _dbContext.SaveChangesAsync();

            //Garantir existência dos Users referenciados
            var userIds = toDosDto.Select(d => d.UserId).Distinct().ToList();
            
            var existingUserIds = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            var missingUserIds = userIds.Except(existingUserIds).ToList();

            if (missingUserIds.Any())
            {
                var newUsers = missingUserIds
                    .Select(id => new User
                    {
                        Id = id
                    }).ToList();

                _dbContext.Users.AddRange(newUsers);

                // Habilita IDENTITY_INSERT para Users (uma vez), salva e desabilita
                // Necessário, já que o JSON de Sync contem os Ids
                await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users ON");
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Users OFF");
            }

            //Inserir todos os ToDos (com os Ids vindos do JSON)
            var toDoEntities = toDosDto.Select(dto => new ToDo
            {
                Id = dto.Id,
                Title = dto.Title,
                IsCompleted = dto.IsCompleted,
                UserId = dto.UserId
            }).ToList();

            _dbContext.ToDos.AddRange(toDoEntities);

            await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.ToDos ON");
            await _dbContext.SaveChangesAsync();
            await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.ToDos OFF");

        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await dbConnection.CloseAsync();
        }
    }
}
