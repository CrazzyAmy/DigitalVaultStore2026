using DigitalProject.Models;
using DigitalProject.Response;
using DigitalProject.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalProject.Security
{
    public class JwtHelper : IJwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthResponse GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtTokenSettings");

            var issuerSigningKey = jwtSettings["IssuerSigningKey"]
                ?? throw new InvalidOperationException("IssuerSigningKey is not configured");
            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("Issuer is not configured");
            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException("Audience is not configured");
            var expireUnitStr = jwtSettings["ExpirationMinutes"]
                ?? throw new InvalidOperationException("ExpirationMinutes is not configured");
            var expireInMin = int.Parse(expireUnitStr);

            // 驗證密鑰長度，HMAC-SHA256 建議至少 32 bytes
            if (Encoding.UTF8.GetBytes(issuerSigningKey).Length < 32)
                throw new ArgumentException("IssuerSigningKey must be at least 32 bytes for HMAC-SHA256");

            // 產生對稱簽章金鑰
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));

            // 設置簽章憑證 - 指定使用 HMAC-SHA256 算法進行簽名
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 設置過期時間（使用 UTC 時間避免時區問題）
            var expiry = DateTime.UtcNow.AddMinutes(expireInMin);

            // 建立 JWT Token 的 Claims（聲明）集合
            // JWT Token 是 Base64 編碼，不是加密，任何人都可以解碼查看 Claims 內容
            // 因此，不應在 Claims 中存放敏感資訊，如密碼、信用卡號等
            var claims = new List<Claim>
            {
                // JWT 標準聲明，代表「主體」，通常用來存放使用者的唯一識別碼
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                // 電子郵件
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // JWT ID，可用於防止 Token 重複使用、實作 Token 撤銷機制
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                // 使用者顯示名稱
                new Claim(ClaimTypes.Name,               user.DisplayName),
                // 使用者角色，用於權限控制
                new Claim(ClaimTypes.Role,               user.Role.ToString()),
            };

            // 產生 JWT Token 物件
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials
            );
             //將 JWT Token 物件 轉換成 字串
            var tokenHandler = new JwtSecurityTokenHandler();
            var encodedToken = tokenHandler.WriteToken(token);


            // 將 JWT Token 物件轉換成字串
            // 將 JWT Token 物件轉換成字串並包裝成 AuthResponse 回傳
            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role.ToString()
            };
        }
    }
}