using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace GusakovUP.Services
{
    public class AuthService
    {
        private const int SaltSize = 16;
        private const int HashSize = 48;
        private const int Iterations = 100000;

        public static byte[] HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] salt = algorithm.Salt;
                byte[] hash = algorithm.GetBytes(HashSize);
                byte[] result = new byte[SaltSize + HashSize];
                Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
                Buffer.BlockCopy(hash, 0, result, SaltSize, HashSize);
                return result;
            }
        }

        public static bool VerifyPassword(string password, byte[] storedPasswordBytes)
        {
            if (storedPasswordBytes == null || storedPasswordBytes.Length != SaltSize + HashSize)
                return false;

            byte[] salt = new byte[SaltSize];
            byte[] hash = new byte[HashSize];
            Buffer.BlockCopy(storedPasswordBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(storedPasswordBytes, SaltSize, hash, 0, HashSize);

            using (var algorithm = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] testHash = algorithm.GetBytes(HashSize);
                int diff = hash.Length ^ testHash.Length;
                for (int i = 0; i < hash.Length && i < testHash.Length; i++)
                {
                    diff |= hash[i] ^ testHash[i];
                }
                return diff == 0;
            }
        }
    }
}
