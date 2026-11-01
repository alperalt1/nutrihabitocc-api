using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutrihabitocc.Infraestructure.Persistence;

namespace Nutrihabitocc.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("health")]
public class HealthController: ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        await _context.Users.AsNoTracking().AnyAsync(cancellationToken);

        return Ok(new
        {
            status = "ok",
            database = "ok",
            utc = DateTime.UtcNow
        });
    }
}
