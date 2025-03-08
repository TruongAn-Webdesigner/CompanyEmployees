using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Entities.Exceptions;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service
{
    //27.3 triển khai tạo user identity
    internal sealed class AuthenticationService : IAuthenticationService
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        //private readonly IConfiguration _configuration; do dùng IOption nên không cần
        //private readonly IOptions<JwtConfiguration> _configuration; //29.2.1 dùng IOption
        private readonly IOptionsSnapshot<JwtConfiguration> _configuration; //29.2.2 reload lại cấu hình mà không dừng app
        private readonly JwtConfiguration _jwtConfiguration;

        private User? _user;

        public AuthenticationService(
            ILoggerManager logger, IMapper mapper, UserManager<User> userManager, IOptionsSnapshot<JwtConfiguration> configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            //29.2.2 dùng IOptionsSnapshot hoặc IOptionsMonitor
            _jwtConfiguration = _configuration.Get("JwtSettings"); // có thể dùng get
            //_jwtConfiguration = _configuration.Value; // có thể dùng value để trích xuất đối tượng JwtConfiguration cho các thuộc tính được điền
            //_jwtConfiguration = new JwtConfiguration();
            //_configuration.Bind(_jwtConfiguration.Section, _jwtConfiguration); do dùng IOption nên không cần
        }

        public async Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration)
        {
            var user = _mapper.Map<User>(userForRegistration);

            var result = await _userManager.CreateAsync(user, userForRegistration.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, userForRegistration.Roles);
            }

            return result;
        }

        // 27.8 triển khai Authentication
        public async Task<bool> ValidateUser(UserForAuthenticationDto userForAuth)
        {
            _user = await _userManager.FindByNameAsync(userForAuth.UserName);

            // hàm CheckPasswordAsync sẽ giúp kiểm tra pass từ hashed
            var result = (_user != null && await _userManager.CheckPasswordAsync(_user, userForAuth.Password));
            if (!result)
                _logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong user name or password.");
            // trả về user nếu tồn tại và pass khớp trong database
            return result;
        }

        // phần triển khai token
        // 28.2 string -> token dto
        public async Task<TokenDto> CreateToken(bool populateExp)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            //28.2
            var resfreshToken = GenerateRefeshToken();

            _user.RefreshToken = resfreshToken;

            if (populateExp)
                _user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            
            await _userManager.UpdateAsync(_user);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return new TokenDto(accessToken, resfreshToken);

            //return new JwtSecurityTokenHandler().WriteToken(tokenOptions); phần triển khai token cũ
        }

        // trả về mảng  byte với thuật toán bảo mật
        private SigningCredentials GetSigningCredentials()
        {
            // lưu ý SECRET = CodeMazeSecretKey này chỉ có 17byte hãy tạo lại mã secret 32 byte cho SecurityAlgorithms.HmacSha256
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET")); 
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        // trả về user name vad role
        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            // 29.1 do dùng binding nên ko cần nữa
            //var jwtSettings = _configuration.GetSection("JwtSettings");

            var tokenOptions = new JwtSecurityToken
                (
                issuer: _jwtConfiguration.ValidIssuer,
                audience: _jwtConfiguration.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtConfiguration.Expires)),
                signingCredentials: signingCredentials
                );
            return tokenOptions;
        }

        //28.2 tạo refesh token
        private string GenerateRefeshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        // dùng để lấy token hết hạn
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            // 29.1 do dùng binding nên ko cần nữa
            //var jwtSettings = _configuration.GetSection("JwtSettings");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"))),
                ValidateLifetime = true,
                ValidIssuer = _jwtConfiguration.ValidIssuer,
                ValidAudience = _jwtConfiguration.ValidAudience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwrSecurityToken = securityToken as JwtSecurityToken;
            if (jwrSecurityToken == null || !jwrSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        //28.3 triển khai lấy refresh token
        public async Task<TokenDto> RefreshToken(TokenDto tokenDto)
        {
            var principal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);

            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null || user.RefreshToken != tokenDto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new RefreshTokenBadRequest();

            _user = user;

            return await CreateToken(populateExp: false);
        }
    }
}
