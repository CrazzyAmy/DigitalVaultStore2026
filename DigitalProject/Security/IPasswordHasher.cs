using System.Security.Cryptography;

namespace DigitalProject.Security
{
    public interface IPasswordHasher
    {
        // 產生隨機鹽值
        // 輸入: size = 欲產生的位元組數
        // 輸出: 隨機的 byte[] 鹽值
        public byte[] GenerateRandomSalt(int size)
        {
            var salt = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}