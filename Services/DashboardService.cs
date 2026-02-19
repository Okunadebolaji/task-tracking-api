using Microsoft.EntityFrameworkCore;
using System.Data;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;

public class DashboardService : IDashboardService
{
    private readonly TaskTrackingDbContext _db;

    public DashboardService(TaskTrackingDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardCountersDto> GetCountersAsync(int companyId)
    {
        var result = new DashboardCountersDto();

        // Using Entity Framework's Database Connection
        var connection = _db.Database.GetDbConnection();

        // Note: Do not wrap 'connection' in an 'await using' block if it's managed by DbContext
        // unless you manually opened it and intend to close it immediately.
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        
        // Calling the PostgreSQL function as a SELECT statement
        command.CommandText = "SELECT * FROM get_dashboard_task_counters(@p0)";
        command.CommandType = CommandType.Text;

        var param = command.CreateParameter();
        param.ParameterName = "@p0";
        param.Value = companyId;
        command.Parameters.Add(param);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            // Mapping the columns returned by the PostgreSQL function
            result.Pending = reader.GetInt32(0);
            result.WIP = reader.GetInt32(1);
            result.Completed = reader.GetInt32(2);
            result.Overdue = reader.GetInt32(3);
        }

        return result;
    }
}