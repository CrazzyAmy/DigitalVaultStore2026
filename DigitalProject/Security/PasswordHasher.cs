using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System;
using System.Text;
using Konscious.Security.Cryptography;
using DigitalProject.Security;
using System.Drawing;
using System.Xml;
using System.Text.Json;
using System.Linq.Expressions;
using DigitalProject.Security;

namespace DigitalProject.Security
{
    /// <summary>
    /// Argon2id 密碼雜湊器
    /// 儲存格式：{salt_hex}:{hash_hex}，直接存入 User.PasswordHash 欄位
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        // ── Argon2id 參數（OWASP 建議值）────────────────────────────────────
        private const int SaltSize = 16;  // 128 bit
        private const int HashSize = 32;  // 256 bit
        private const int Iterations = 4;
        private const int MemorySize = 65536; // 64 MB
        private const int DegreeOfParallelism = 4;

        public byte[] GenerateRandomSalt(int size)
        {
            var salt = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        /// <summary>
        /// 雜湊密碼，回傳 "{salt_hex}:{hash_hex}"
        /// </summary>
        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = ComputeHash(password, salt);
            return $"{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
        }

        /// <summary>
        /// 驗證密碼，storedHash 格式為 "{salt_hex}:{hash_hex}"
        /// </summary>
        public bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt;
            byte[] expectedHash;

            try
            {
                salt = Convert.FromHexString(parts[0]);
                expectedHash = Convert.FromHexString(parts[1]);
            }
            catch
            {
                return false;
            }

            var actualHash = ComputeHash(password, salt);

            // 使用 constant-time 比較，防止 timing attack
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                Iterations = Iterations,
                MemorySize = MemorySize,
            };
            return argon2.GetBytes(HashSize);
        }
    }
}