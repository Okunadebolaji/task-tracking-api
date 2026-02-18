using Microsoft.Data.SqlClient;
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

        var connection = _db.Database.GetDbConnection();

        await using (connection)
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "sp_Dashboard_TaskCounters";
            command.CommandType = CommandType.StoredProcedure;

            var param = command.CreateParameter();
            param.ParameterName = "@CompanyId";
            param.Value = companyId;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.Pending = reader.GetInt32(0);
                result.WIP = reader.GetInt32(1);
                result.Completed = reader.GetInt32(2);
                result.Overdue = reader.GetInt32(3);
            }
        }

        return result;
    }
}
