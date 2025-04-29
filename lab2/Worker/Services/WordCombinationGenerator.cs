// Worker/Services/WordCombinationGenerator.cs

namespace Worker.Services;

public class WordCombinationGenerator
{
    public IEnumerable<string> GenerateWords(
        int targetLength, string alphabet, int partNumber, int partCount)
    {
        var aLen = alphabet.Length;
        var total = (long)Math.Pow(aLen, targetLength);

        for (long global = partNumber - 1; global < total; global += partCount)
        {
            var idx = global;
            var chars = new char[targetLength];

            for (var pos = targetLength - 1; pos >= 0; pos--)
            {
                chars[pos] = alphabet[(int)(idx % aLen)];
                idx /= aLen;
            }

            yield return new string(chars);
        }
    }
}