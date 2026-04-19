using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RustyDagger.Data;
using RustyDagger.Shared.Dto;

namespace RustyDagger.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RankingController : ControllerBase
{
    private readonly DragonCourtDbContext _db;

    public RankingController(DragonCourtDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<RankingEntry>>> GetRankings([FromQuery] int top = 20)
    {
        var rankings = await _db.Rankings
            .OrderByDescending(r => r.Fame)
            .Take(Math.Min(top, 100))
            .Select(r => new RankingEntry
            {
                HeroName = r.HeroName,
                AccountName = r.AccountName,
                Level = r.Level,
                Fame = r.Fame,
                Guts = r.Guts,
                Wits = r.Wits,
                Charm = r.Charm
            })
            .ToListAsync();

        return Ok(rankings);
    }
}
