//Worker/Services/HashCrackService.cs

using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Worker.Interfaces;
using Worker.Models;

namespace Worker.Services;

public class HashCrackService : IHashCrackService
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
    private readonly WordCombinationGenerator _generator = new();

    public List<string> BruteForce(
        string md5Hash,
        int maxLength,
        int partNumber,
        int partCount)
    {
        var found = new List<string>();
        
        for (int length = 1; length <= maxLength; length++)
        {
            foreach (var word in _generator.GenerateWords(length, Alphabet, partNumber, partCount))
            {
                var hash = GetMd5(word);
                if (string.Equals(hash, md5Hash, System.StringComparison.OrdinalIgnoreCase))
                {
                    found.Add(word);
                }
            }
        }
        return found;
    }
    
    private static string GetMd5(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = MD5.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
