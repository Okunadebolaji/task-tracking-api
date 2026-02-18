using Microsoft.AspNetCore.Mvc;
using TaskTrackingApi.Models;
using TaskTrackingApi.Dtos;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly TaskTrackingDbContext _db;
    private readonly ICompanyCodeGenerator _codeGenerator;
    private readonly IPermissionService _permissionService;

    public CompaniesController(TaskTrackingDbContext db,ICompanyCodeGenerator codeGenerator,IPermissionService
    permissionService)
    {
        _db = db;
        _codeGenerator = codeGenerator;
        _permissionService = permissionService;
    }

         // GET: api/teams/all?companyId=1
[HttpGet("all")]
public async Task<IActionResult> GetAllCompanyTeams([FromQuery] int companyId)
{
    var teams = await _db.Teams
        .Where(t => t.CompanyId == companyId)
        .ToListAsync();

    return Ok(teams);
}



    [HttpPost]
    public async Task<IActionResult> Create(CreateCompanyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Company name is required");

        var code = await _codeGenerator.GenerateAsync(dto.Name);

        var company = new Company
        {
            Name = dto.Name,
            Code = code
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync();

        return Ok(company);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_db.Companies.ToList());
    }


//Get company profile
 [HttpGet("profile")]
public async Task<IActionResult> GetProfile(
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    

    var company = await _db.Companies
        .Where(c => c.Id == user.CompanyId)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Code,
            c.CreatedAt
        })
        .FirstOrDefaultAsync();

    if (company == null)
        return NotFound("Company not found");

    return Ok(company);
}


//Update company profile

         [HttpPut("profile")]
public async Task<IActionResult> UpdateProfile(
    [FromBody] UpdateCompanyDto dto,
    [FromHeader(Name = "x-user-id")] string userIdHeader)
{
    if (!int.TryParse(userIdHeader, out var userId))
        return BadRequest("Invalid x-user-id");

    if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Company name is required");

    var user = await _db.Users.FindAsync(userId);
    if (user == null) return Unauthorized();

    

      

    var company = await _db.Companies
        .FirstOrDefaultAsync(c => c.Id == user.CompanyId);

    if (company == null)
        return NotFound("Company not found");

    company.Name = dto.Name;

    await _db.SaveChangesAsync();

    return Ok(new
    {
        company.Id,
        company.Name,
        company.Code
    });
}


  
    //  Code generator HELPER

    private async Task<string> GenerateCompanyCode(string companyName)
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

        var count = await _db.Companies
            .CountAsync(c => c.Code.StartsWith($"{prefix}-{year}"));

        var sequence = (count + 1).ToString("D3");

        return $"{prefix}-{year}-{sequence}";
    }
}
