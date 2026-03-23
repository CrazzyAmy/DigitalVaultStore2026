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

namespace DigitalProject.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int _SALT_SIZE = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int _ITERATIONS = 4;
        private const int _MEMORY_SIZE= 65536; // 64 MB
        private const int _DEGREE_OF_PARALLELISM = 4;
        private const int _HASH_SIZE= 32; // 256 bit

        private readonly Dictionary<string, DigitalProject.Models.UserLoginInfo> _users = new();
        //private readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;
        private byte[] CreateSalt()
        {
            return RandomNumberGenerator.GetBytes(_SALT_SIZE);
        }
        static string HashPassword(string password, byte[] salt, int memoryKb, int iterations, int degreeOfParallelism, int hashLength)
        {
            byte[] pwBytes = Encoding.UTF8.GetBytes(password);
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _DEGREE_OF_PARALLELISM,
                Iterations = _ITERATIONS,
                MemorySize = _MEMORY_SIZE
            };
            byte[] hash = argon2.GetBytes(_HASH_SIZE);
            return Convert.ToHexString(hash);
        }

        public bool VerifyPassword(string email, string password)
        {
            if (!_users.ContainsKey(email)){
                return false;
            }
            var user = _users[email];
            byte[] salt = Convert.FromHexString(user.Salt);
            string hashPassword = HashPassword(password, salt, _MEMORY_SIZE, _ITERATIONS, _DEGREE_OF_PARALLELISM, _HASH_SIZE);
            return user.PasswordHash == hashPassword;
        }
        public void CreateUser(string email, string password)
        {
            var salt = CreateSalt();
            string hashPassword = HashPassword(password, salt, _MEMORY_SIZE, _ITERATIONS, _DEGREE_OF_PARALLELISM, _HASH_SIZE);

            var user = new DigitalProject.Models.UserLoginInfo(email, Convert.ToHexString(salt), hashPassword);
            _users[email] = user;
            Save();
        }

        private void Save()
        {
            // 在這裡實現將 _users 字典保存到持久化存儲的邏輯，例如寫入文件或數據庫
            var userEntries = _users.Values.Select(u => new
            {
                u.Email,
                u.Salt,
                u.PasswordHash
            }).ToList();

            string json = System.Text.Json.JsonSerializer.Serialize(userEntries, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            string tempFileName = _FILE + ".tmp";
            File.WriteAllText(tempFileName, json);
            File.Move(tempFileName, _FILE, true);
        }
        private const string _FILE = "users";
        public void Load()
        {
            if (!File.Exists(_FILE))
            {
                return;
            }
            try
            {
                string json = File.ReadAllText(_FILE);
                var userEntries = System.Text.Json.JsonSerializer.Deserialize<List<DigitalProject.Models.UserLoginInfo>>(json);

                _users.Clear();
                foreach (var user in userEntries)
                {
                    _users[user.Email] = user;
                }
            } catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse user data from {_FILE}. The file may be corrupted. Details: {ex.Message}");
            }
        }

                

        public static byte[] GenerateRandomSalt(int v)
        {
            var salt = new byte[KeySize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
