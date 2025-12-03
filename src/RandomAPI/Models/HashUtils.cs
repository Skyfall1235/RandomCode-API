using System.Security.Cryptography;
using System.Text;

namespace RandomAPI.Models
{
    /// <summary>
    /// Provides utility methods for generating short, unique hashes.
    /// </summary>
    public static class HashUtils
    {
        /// <summary>
        /// Generates a unique, short (8-character) alphanumeric hash from an input string.
        /// This is useful for creating human-readable internal IDs from larger payloads.
        /// </summary>
        /// <param name="input">The string to hash (e.g., the raw JSON payload, or a combination of fields).</param>
        /// <returns>An 8-character alphanumeric hash string.</returns>
        public static string GenerateShortHash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            using SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(inputBytes);
            var base64Hash = Convert.ToBase64String(hashBytes);
            var shortHash = base64Hash
                .Replace("/", "_") // Replace dangerous chars with safe ones
                .Replace("+", "-")
                .Replace("=", "")[..8]; // Take the first 8 chars for the short identifier
            return shortHash;
        }
    }
}
