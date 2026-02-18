using TaskTrackingApi.Dtos;

public interface IDashboardService
{
    Task<DashboardCountersDto> GetCountersAsync(int companyId);
}
