using Microsoft.AspNetCore.Mvc;
using KogaseEngine.Core.Dtos.Auth;
using KogaseEngine.Core.Services.Auth;
using KogaseEngine.Core.Services.Iam;

namespace KogaseEngine.Api.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly UserService _userService;
    private readonly DeviceService _deviceService;

    public AuthController(
        AuthService authService,
        UserService userService,
        DeviceService deviceService
    )
    {
        _authService = authService;
        _userService = userService;
        _deviceService = deviceService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto loginDto)
    {
        try
        {
            var (token, refreshToken, expiresAt) = 
                await _authService.AuthenticateUserAsync(loginDto.Email, loginDto.Password, loginDto.DeviceId);
            
            var user = await _userService.GetUserByEmailAsync(loginDto.Email);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(RefreshTokenRequestDto refreshDto)
    {
        try
        {
            var (token, refreshToken, expiresAt) = await _authService.RefreshTokenAsync(refreshDto.RefreshToken);

            return Ok(new RefreshTokenResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromHeader(Name = "Authorization")] string authorization)
    {
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            return BadRequest(new { message = "Invalid authorization header" });

        var token = authorization.Substring("Bearer ".Length).Trim();
        await _authService.RevokeTokenAsync(token);
        
        return NoContent();
    }

    [HttpPost("link/device")]
    public async Task<ActionResult> LinkDeviceToUser(LinkDeviceRequestDto linkDto)
    {
        try
        {
            // Verify user exists
            var user = await _userService.GetUserByIdAsync(linkDto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Verify device exists
            var device = await _deviceService.GetDeviceByIdAsync(linkDto.DeviceId);
            if (device == null)
                return NotFound(new { message = "Device not found" });

            // The actual linking is done implicitly through sessions and auth tokens
            // This endpoint exists primarily for documentation and future extensions
            
            return Ok(new { message = "Device linked successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("link/device/{deviceId:guid}")]
    public async Task<ActionResult> UnlinkDevice(Guid deviceId)
    {
        try
        {
            // Verify device exists
            var device = await _deviceService.GetDeviceByIdAsync(deviceId);
            if (device == null)
                return NotFound(new { message = "Device not found" });

            // Revoke all tokens for this device
            await _authService.RevokeAllDeviceTokensAsync(deviceId);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}