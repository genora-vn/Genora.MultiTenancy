using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Genora.MultiTenancy.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class MultiTenancyDbContextFactory : IDesignTimeDbContextFactory<MultiTenancyDbContext>
{
    public MultiTenancyDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        MultiTenancyEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<MultiTenancyDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new MultiTenancyDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Genora.MultiTenancy.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
