using System;
using System.Collections.Generic;

namespace Worker.Models;

public class WordCombinationGenerator
{
    public IEnumerable<string> GenerateWords(int targetLength, string alphabet, int partNumber, int partCount)
    {
        int segmentSize = alphabet.Length / partCount;
        int remainder = alphabet.Length % partCount;
        int workerIndex = partNumber - 1;
        int rangeStart = workerIndex * segmentSize + Math.Min(workerIndex, remainder);
        int rangeLength = segmentSize + (workerIndex < remainder ? 1 : 0);
        int rangeEnd = rangeStart + rangeLength - 1;

        for (int i = rangeStart; i <= rangeEnd; i++)
        {
            char firstChar = alphabet[i];
            if (targetLength == 1)
            {
                yield return firstChar.ToString();
            }
            else
            {
                foreach (var suffix in GenerateSuffixes(targetLength - 1, alphabet))
                {
                    yield return firstChar + suffix;
                }
            }
        }
    }
    
    private IEnumerable<string> GenerateSuffixes(int length, string alphabet)
    {
        int n = alphabet.Length;
        int[] indices = new int[length];

        while (true)
        {
            char[] chars = new char[length];
            for (int j = 0; j < length; j++)
            {
                chars[j] = alphabet[indices[j]];
            }

            yield return new string(chars);
            
            var pos = length - 1;
            while (pos >= 0)
            {
                if (indices[pos] < n - 1)
                {
                    indices[pos]++;
                    {
                        break;
                    }
                }

                indices[pos] = 0;
                pos--;
            }

            if (pos < 0)
            {
                break;
            }
        }
    }
}