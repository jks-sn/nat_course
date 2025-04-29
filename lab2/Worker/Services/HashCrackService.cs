// Worker/Services/HashCrackService.cs

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Worker.Services;

public class HashCrackService(ILogger<HashCrackService> logger)
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
    private readonly WordCombinationGenerator _gen = new();

    public List<string> BruteForce(string md5, int maxLen, int partNumber, int partCount, CancellationToken ct)
    {
        var found = new List<string>();
        long countChecked = 0;
        var sw = Stopwatch.StartNew();
        for (var len = 1; len <= maxLen; len++)
        {
            foreach (var word in _gen.GenerateWords(len, Alphabet, partNumber, partCount))
            {
                if (ct.IsCancellationRequested) 
                    break;
                
                countChecked++;
                if (GetMd5(word) == md5)
                    found.Add(word);
                
                if (countChecked % 5000000 == 0)
                {
                    logger.LogInformation("Task {Task}: length={Len}, checked={Checked:n0} ({RPS:n0}/s)", partNumber, len, countChecked, countChecked / sw.Elapsed.TotalSeconds);
                }
            }
        }
        logger.LogInformation("Task {Task} DONE: {Checked:n0} checked in {Elapsed}", partNumber, countChecked, sw.Elapsed);
        
        return found;
    }

    private static string GetMd5(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}