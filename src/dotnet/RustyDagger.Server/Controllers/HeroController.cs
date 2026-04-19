using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RustyDagger.Server.Services;
using RustyDagger.Shared.Dto;

namespace RustyDagger.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HeroController : ControllerBase
{
    private readonly HeroService _heroService;

    public HeroController(HeroService heroService) => _heroService = heroService;

    private int AccountId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<List<HeroSummary>>> GetHeroes()
    {
        var heroes = await _heroService.GetHeroesAsync(AccountId);
        return Ok(heroes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HeroSummary>> GetHero(int id)
    {
        var entity = await _heroService.GetHeroAsync(id, AccountId);
        if (entity == null) return NotFound();

        return Ok(new HeroSummary
        {
            Id = entity.Id,
            Name = entity.Name,
            Level = entity.Level,
            Guts = entity.Guts,
            Wits = entity.Wits,
            Charm = entity.Charm,
            Fame = entity.Fame,
            Place = entity.Place
        });
    }

    [HttpPost]
    public async Task<ActionResult<HeroSummary>> CreateHero([FromBody] CreateHeroRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var entity = await _heroService.CreateHeroAsync(AccountId, req);
            return CreatedAtAction(nameof(GetHero), new { id = entity.Id }, new HeroSummary
            {
                Id = entity.Id,
                Name = entity.Name,
                Level = entity.Level,
                Guts = entity.Guts,
                Wits = entity.Wits,
                Charm = entity.Charm,
                Fame = entity.Fame,
                Place = entity.Place
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
