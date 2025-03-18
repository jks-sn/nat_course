//Worker/Interfaces/IHashCrackService.cs

namespace Worker.Interfaces;

public interface IHashCrackService
{
    List<string> BruteForce(
        string md5Hash,
        int maxLength,
        int partNumber,
        int partCount
    );
}