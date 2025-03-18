//Tests/HashCrackServiceTests.cs

using FluentAssertions;
using Worker.Services;

// Можно подключить FluentAssertions для удобства (не обязательно).

namespace Tests;

public class HashCrackServiceTests
{
    private readonly HashCrackService service = new();

    [Fact]
    public void BruteForce_FindsAbcd_WhenSinglePartCoversAll()
    {
        // "abcd" => e2fc714c4727ee9395f324cd2e7f331f
        var md5 = "e2fc714c4727ee9395f324cd2e7f331f";

        var found = service.BruteForce(md5, maxLength: 4, partNumber: 1, partCount: 1);

        found.Should().Contain("abcd");
    }

    [Fact]
    public void BruteForce_DoesNotFindAbcd_WhenPartNumberDoesntCoverA()
    {

        var md5 = "e2fc714c4727ee9395f324cd2e7f331f";
        var found = service.BruteForce(md5, maxLength: 4, partNumber: 2, partCount: 4);

        found.Should().NotContain("abcd");
    }
}