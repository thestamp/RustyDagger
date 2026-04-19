using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RustyDagger.Data;
using RustyDagger.Data.Entities;
using RustyDagger.Server.Services;
using RustyDagger.Shared.Dto;

namespace RustyDagger.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DragonCourtDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(DragonCourtDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse { Error = "Invalid request." });

        var existing = await _db.Accounts
            .AnyAsync(a => a.Name == req.Name);
        if (existing)
            return Conflict(new AuthResponse { Error = "Name already taken." });

        var account = new AccountEntity
        {
            Name = req.Name.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(account.Id, account.Name);
        return Ok(new AuthResponse { Token = token, Name = account.Name, Success = true });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse { Error = "Invalid request." });

        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.Name == req.Name);
        if (account == null || !BCrypt.Net.BCrypt.Verify(req.Password, account.PasswordHash))
            return Unauthorized(new AuthResponse { Error = "Invalid name or password." });

        account.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(account.Id, account.Name);
        return Ok(new AuthResponse { Token = token, Name = account.Name, Success = true });
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AuthResponse> Me()
    {
        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        return Ok(new AuthResponse { Name = name, Success = true });
    }
}
