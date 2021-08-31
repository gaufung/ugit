using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tindo.Ugit.Server.Models;

namespace Tindo.Ugit.Server
{
    public class UgitDatabaseContext : DbContext
    {
        private IConfiguration _configuration;

        public DbSet<Repository> Repositories { get; set; }

        public UgitDatabaseContext(IConfiguration configuraiton)
        {
            _configuration = configuraiton;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={_configuration.GetValue<string>("ConnectionStrings")}");
    }
}
