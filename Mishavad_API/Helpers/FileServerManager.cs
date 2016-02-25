using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;
using Mishavad_API.Models;
namespace Mishavad_API.Helpers
{
    public static class FileServerTokenManager
    {
        public static TimeSpan TokenTimeSpan;
        private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

        public static string GenerateRandomToken()
        {
            const int bitsPerByte = 8;
            const int strengthInBits = 256;

            int strengthInBytes = strengthInBits / bitsPerByte;

            byte[] data = new byte[strengthInBytes];
            _random.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        
        public static string GenerateHash(string fileServerToken) {

            /* NOTE: We use no-salt hash for creating hashes of File Server Tokens, because:
            1) Their security is not as crucial as user passwords
            2) Hackers probably are less interested in hacking them
                They are interested if we use the same strategy to download documents
                TODOs:
                
            3) They are short-time in nature */
           
            byte[] databytes = Convert.FromBase64String(fileServerToken);
            byte[] hashbytes;

            using (var shaM = new SHA512Managed()) {
                hashbytes = shaM.ComputeHash(databytes);
            }

            return Convert.ToBase64String(hashbytes);
        }


        // 24 = 192 bits
        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int HasingIterationsCount = 10101;


        public static string GenerateSaltedHash(string token)
        {
            // http://stackoverflow.com/questions/19957176/asp-net-identity-password-hashing

            byte[] salt;
            byte[] buffer2;
            if (token==null)
            {
                throw new ArgumentNullException("token");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(token, SaltByteSize, HasingIterationsCount))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(HashByteSize);
            }
            byte[] dst = new byte[(SaltByteSize + HashByteSize) + 1];
            Buffer.BlockCopy(salt, 0, dst, 1, SaltByteSize);
            Buffer.BlockCopy(buffer2, 0, dst, SaltByteSize + 1, HashByteSize);
            return Convert.ToBase64String(dst);
        }

        public static bool VerifySaltedHash(string hash, string token)
        {
            byte[] _passwordHashBytes;

            int _arrayLen = (SaltByteSize + HashByteSize) + 1;

            if (hash == null)
            {
                return false;
            }

            if (token == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] src = Convert.FromBase64String(hash);

            if ((src.Length != _arrayLen) || (src[0] != 0))
            {
                return false;
            }

            byte[] _currentSaltBytes = new byte[SaltByteSize];
            Buffer.BlockCopy(src, 1, _currentSaltBytes, 0, SaltByteSize);

            byte[] _currentHashBytes = new byte[HashByteSize];
            Buffer.BlockCopy(src, SaltByteSize + 1, _currentHashBytes, 0, HashByteSize);

            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(token, _currentSaltBytes, HasingIterationsCount))
            {
                _passwordHashBytes = bytes.GetBytes(SaltByteSize);
            }

            return AreSaltedHashesEqual(_currentHashBytes, _passwordHashBytes);

        }

        private static bool AreSaltedHashesEqual(byte[] firstHash, byte[] secondHash)
        {
            int _minHashLength = firstHash.Length <= secondHash.Length ? firstHash.Length : secondHash.Length;
            var xor = firstHash.Length ^ secondHash.Length;
            for (int i = 0; i < _minHashLength; i++)
                xor |= firstHash[i] ^ secondHash[i];
            return 0 == xor;
        }


       
            public static string GetFullPath(FileServer fileServer, string path)
            {
                return (fileServer == null || path == null) ? null : string.Format("http://{0}/{1}", fileServer.ServerUri ?? fileServer.ServerIP , path);
            }
        
    }
}