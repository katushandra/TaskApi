using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace TaskApi.Helpers
{
    public class PasswordHelper
    {
        public static byte[] GetSecureSalt()
        {
            return RandomNumberGenerator.GetBytes(32);
        }

        public static string HashPasswordPbkdf2(string password, byte[] salt)
        {
            byte[] derivedKey = KeyDerivation.Pbkdf2( password, salt,KeyDerivationPrf.HMACSHA256, iterationCount: 300000, 32);
            return Convert.ToBase64String(derivedKey);
        }

        internal static string HashUsingPbkdf2(string password, byte[] salt)
        {
            throw new NotImplementedException();
        }
    }
}
