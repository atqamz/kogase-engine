using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KogaseEngine.Domain.Entities.Auth;
using KogaseEngine.Domain.Entities.Iam;
using KogaseEngine.Domain.Interfaces;
using KogaseEngine.Domain.Interfaces.Auth;
using KogaseEngine.Domain.Interfaces.Iam;
using Microsoft.IdentityModel.Tokens;

namespace KogaseEngine.Core.Services.Auth;

public class AuthService
{
    private readonly IAuthTokenRepository _authTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _jwtSecret;
    private readonly int _tokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public AuthService(
        IAuthTokenRepository authTokenRepository,
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        ISessionRepository sessionRepository,
        IUnitOfWork unitOfWork,
        string jwtSecret,
        int tokenExpirationMinutes = 60,
        int refreshTokenExpirationDays = 7
    )
    {
        _authTokenRepository = authTokenRepository;
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
        _jwtSecret = jwtSecret;
        _tokenExpirationMinutes = tokenExpirationMinutes;
        _refreshTokenExpirationDays = refreshTokenExpirationDays;
    }

    public async Task<(string Token, string RefreshToken, DateTime ExpiresAt)> AuthenticateUserAsync(string email, string password, Guid? deviceId = null)
    {
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("Invalid credentials.");

        // Verify password
        if (!VerifyPassword(password, user.PasswordHash))
            throw new InvalidOperationException("Invalid credentials.");

        // Check if user is active
        if (user.Status != UserStatus.Active)
            throw new InvalidOperationException("User account is not active.");

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate tokens
        var (token, refreshToken, expiresAt) = await GenerateTokensForUserAsync(user.Id, deviceId);
        
        await _unitOfWork.SaveChangesAsync();

        return (token, refreshToken, expiresAt);
    }

    public async Task<(string Token, string RefreshToken, DateTime ExpiresAt)> RefreshTokenAsync(string refreshToken)
    {
        // Find refresh token
        var authToken = await _authTokenRepository.GetByRefreshTokenAsync(refreshToken);
        if (authToken == null)
            throw new InvalidOperationException("Invalid refresh token.");

        // Check if user is active
        if (authToken.User?.Status != UserStatus.Active)
            throw new InvalidOperationException("User account is not active.");

        // Revoke the current token
        await _authTokenRepository.RevokeAsync(authToken.Id);

        // Generate new tokens
        var (token, newRefreshToken, expiresAt) = await GenerateTokensForUserAsync(authToken.UserId, authToken.DeviceId);
        
        await _unitOfWork.SaveChangesAsync();

        return (token, newRefreshToken, expiresAt);
    }

    public async Task RevokeTokenAsync(string token)
    {
        var authToken = await _authTokenRepository.GetByTokenAsync(token);
        if (authToken != null)
        {
            await _authTokenRepository.RevokeAsync(authToken.Id);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        await _authTokenRepository.RevokeAllForUserAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RevokeAllDeviceTokensAsync(Guid deviceId)
    {
        await _authTokenRepository.RevokeAllForDeviceAsync(deviceId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Guid?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "sub").Value);

            var authToken = await _authTokenRepository.GetByTokenAsync(token);
            if (authToken == null || authToken.RevokedAt != null)
                return null;

            return userId;
        }
        catch
        {
            return null;
        }
    }

    private async Task<(string Token, string RefreshToken, DateTime ExpiresAt)> GenerateTokensForUserAsync(Guid userId, Guid? deviceId)
    {
        // Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var expiresAt = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            { 
                new Claim("sub", userId.ToString())
            }),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);
        
        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        
        // Save to database
        var authToken = new AuthToken
        {
            UserId = userId,
            DeviceId = deviceId,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            IssuedAt = DateTime.UtcNow
        };
        
        await _authTokenRepository.CreateAsync(authToken);
        
        return (token, refreshToken, expiresAt);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToBase64String(hashedBytes);
        return hash == storedHash;
    }
}