using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace ApiGateway.Extentions.Authorization.Services
{
    public class GatewayPasswordHasher<TUser> : PasswordHasher<TUser>, IPasswordHasher<TUser> where TUser : class
    {
        /// <summary>
        /// Verify hashed password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <returns></returns>
        public override PasswordVerificationResult VerifyHashedPassword(
            TUser user,
            string hashedPassword,
            string providedPassword)
        {
            if (IsValidLegacyCredential(hashedPassword, providedPassword))
            {
                return PasswordVerificationResult.Success;
            }

            return base.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }

        private bool IsValidLegacyCredential(string hashedPassword, string providedPassword)
        {
            // Empty crendetials are not valid
            if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
            {
                return false;
            }

            var saltDelimiterIndex = hashedPassword.IndexOf("|");

            string salt;
            string hash;

            // md5 legacy hash
            if (saltDelimiterIndex != -1 && hashedPassword.Length > saltDelimiterIndex)
            {
                salt = hashedPassword.Substring(0, saltDelimiterIndex);
                hash = hashedPassword.Substring(saltDelimiterIndex + 1);

                return IsValidSupportedLegacyCredential(
                        "md5",
                        1,
                        hash,
                        providedPassword,
                        salt);
            }

            // sha512 legacy hash CAN NEVER be less than or equals 40 characters, guaranteed
            if (hashedPassword.Length <= 40)
            {
                return false;
            }

            // Extract salt and actual hash
            // if all looks good, the salt occupy the first 40 characters of the hash
            var saltLength = 40;

            salt = hashedPassword.Substring(0, saltLength);
            hash = hashedPassword.Substring(saltLength);

            return IsValidSupportedLegacyCredential(
                "sha512",
                512,
                hash,
                providedPassword,
                salt);
        }

        private bool IsValidSupportedLegacyCredential(
            string algorithm,
            int iterations,
            string hashedPassword,
            string providedPassword,
            string salt)
        {
            var isValid = false;

            string passwordSalted;
            byte[] saltedBytes;

            switch (algorithm)
            {
                case "md5":
                    // Legacy hash Algorithm 1 details:
                    //   password salted    : providedPassword+salt
                    //   algorithm          : md5
                    //   iterations         : 1
                    //   encoded as         : lowercase base16

                    passwordSalted = $"{providedPassword}{salt}";

                    saltedBytes = Encoding.UTF8.GetBytes(passwordSalted);

                    using (var md5 = MD5.Create())
                    {
                        var digest = md5.ComputeHash(saltedBytes);

                        var outputBytes = new byte[digest.Length + saltedBytes.Length];

                        // Last (iterations - 1) iterations
                        for (var iteration = 1; iteration < iterations; iteration++)
                        {
                            Buffer.BlockCopy(digest, 0, outputBytes, 0, digest.Length);
                            Buffer.BlockCopy(saltedBytes, 0, outputBytes, digest.Length, saltedBytes.Length);

                            digest = md5.ComputeHash(outputBytes);
                        }

                        var builder = new StringBuilder(digest.Length);

                        foreach (var b in digest)
                        {
                            builder.Append(b.ToString("x2"));
                        }

                        var result = builder.ToString();

                        isValid = result == hashedPassword;
                    }
                    break;
                case "sha512":
                    // Legacy hash Algorithm 2 details:
                    //   password salted    : providedPassword+{salt}
                    //   algorithm          : sha512
                    //   iterations         : 512
                    //   encoded as         : base64

                    passwordSalted = $"{providedPassword}{{{salt}}}";

                    saltedBytes = Encoding.UTF8.GetBytes(passwordSalted);

                    using (var sha512 = SHA512.Create())
                    {
                        // First iteration
                        var digest = sha512.ComputeHash(saltedBytes);

                        var outputBytes = new byte[digest.Length + saltedBytes.Length];

                        // Last (iterations - 1) iterations
                        for (var iteration = 1; iteration < iterations; iteration++)
                        {
                            Buffer.BlockCopy(digest, 0, outputBytes, 0, digest.Length);
                            Buffer.BlockCopy(saltedBytes, 0, outputBytes, digest.Length, saltedBytes.Length);

                            digest = sha512.ComputeHash(outputBytes);
                        }

                        var result = Convert.ToBase64String(digest);

                        isValid = result == hashedPassword;
                    }
                    break;
                default:
                    break;
            }

            return isValid;
        }
    }
}
