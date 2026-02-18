using Microsoft.EntityFrameworkCore;
using TaskTrackingApi.Models;

namespace TaskTrackingApi.Models
{
    public interface ICompanyCodeGenerator
    {
        Task<string> GenerateAsync(string companyName);
    }

    public class CompanyCodeGenerator : ICompanyCodeGenerator
    {
        private readonly TaskTrackingDbContext _db;

        public CompanyCodeGenerator(TaskTrackingDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateAsync(string companyName)
        {
            var prefix = new string(
                companyName
                    .Where(char.IsLetter)
                    .Take(3)
                    .ToArray()
            ).ToUpper();

            if (prefix.Length < 3)
                prefix = prefix.PadRight(3, 'X');

            var year = DateTime.UtcNow.Year;

            // Count existing companies with same prefix+year
            var count = await _db.Companies
                .CountAsync(c => c.Code.StartsWith($"{prefix}-{year}"));

            var sequence = (count + 1).ToString("D3");

            return $"{prefix}-{year}-{sequence}";
        }
    }
}
