using ApiGateway.Extentions;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ApiGateway.Auth
{
    /// <summary>
    /// Provide encryption and decryption of ids
    /// </summary>
    public class IdCryptoProvider : IIdCryptoProvider
    {
        private static Random _rnd = new Random();
        private static readonly char[] _base64Padding = { '=' };

        /// <summary>
        /// Decrypt id
        /// </summary>
        /// <param name="encryptedId"></param>
        /// <returns></returns>
        public string Decrypt(string encryptedId)
        {
            if (string.IsNullOrWhiteSpace(encryptedId)) return null;

            string id = null;

            var encodedId = encryptedId.Replace('_', '/').Replace('-', '+');

            var paddingLength = encryptedId.Length % 4;

            // add padding
            if (paddingLength == 2)
            {
                encodedId += "==";
            }
            else if (paddingLength == 3)
            {
                encodedId += "=";
            }

            try
            {
                var idBase64Decoded = encodedId.Base64Decode().Reverse();

                var primeOfOurHashString = idBase64Decoded.Split(new[] { "eyJ" }, StringSplitOptions.None).First();

                if (int.TryParse(primeOfOurHashString, out var primeOfOurHash))
                {
                    var indexOfprimeOfOurHash = idBase64Decoded.IndexOf("eyJ");

                    if (indexOfprimeOfOurHash != -1 && idBase64Decoded.Length > indexOfprimeOfOurHash + 3)
                    {
                        var adIdFirstHash = idBase64Decoded.Substring(indexOfprimeOfOurHash + 3);

                        var adIdFirstHashInHalf = adIdFirstHash.Length % 2 == 0 ? adIdFirstHash.SplitInHalf() :
                            adIdFirstHash.SplitInHalf(firstHalfBigger: true);

                        if (adIdFirstHashInHalf.Length == 2)
                        {
                            adIdFirstHash = string.Concat(adIdFirstHashInHalf[1], adIdFirstHashInHalf[0]);

                            if (adIdFirstHash.Length > primeOfOurHash)
                            {
                                id = adIdFirstHash.Substring(primeOfOurHash);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return id;
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Encrypt(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id), "value can not be null");

            var maxIdPrefixSize = id.Length >= 32 ? id.Length + 1 : 32;

            var idPrefixSize = _rnd.Next(1, maxIdPrefixSize);

            var idPrefix = CreateSalt(idPrefixSize);

            var idFirstCode = $"{idPrefix}{id}"
                .Base64Encode();

            var primeOfOurHash = PrimeOfOurHash(idFirstCode.Length);

            var salt = CreateSalt(primeOfOurHash);

            idFirstCode = string.Concat(salt, id);

            var idFirstCodeInHalf = idFirstCode.SplitInHalf();

            var idCode = string.Concat(salt.Length.ToString(), "eyJ", idFirstCodeInHalf[1], idFirstCodeInHalf[0]);

            idCode = idCode.Reverse().Base64Encode();

            // url friendly
            idCode = idCode.TrimEnd(_base64Padding).Replace("+", "-").Replace("/", "_");

            return idCode;
        }

        private static string CreateSalt(int maxSize)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            byte[] data = new byte[maxSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }

            var result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        private static bool Isprime(int candidate)
        {
            if (candidate < 2)
                return false;

            for (var divisor = 2; divisor <= Math.Sqrt(candidate); divisor++)
            {
                if (candidate % divisor == 0)
                    return false;
            }
            return true;
        }

        private static int PrimeOfOurHash(int length)
        {
            var primeOfOurHash = length;

            if (length <= 0)
                throw new InvalidOperationException("length can not be 0");

            for (var i = length; i > 0; i--)
            {
                if (Isprime(i))
                    return i;
            }

            return primeOfOurHash;
        }
    }
}
