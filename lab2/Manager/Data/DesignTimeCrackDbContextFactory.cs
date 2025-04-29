// Manager/Data/DesignTimeCrackDbContextFactory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace Manager.Data;

public class DesignTimeCrackDbContextFactory : IDesignTimeDbContextFactory<CrackDbContext>
{
    public CrackDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CrackDbContext>();
        NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
        optionsBuilder.UseNpgsql("Host=localhost;Database=crackhash;Username=postgres;Password=postgres");
        return new CrackDbContext(optionsBuilder.Options);
    }
}