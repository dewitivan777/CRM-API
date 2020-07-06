using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Model;

namespace AuthService
{
    public class SqlDbContext : DbContext
    {
        /// <summary>
        /// Set SqlDbContext options
        /// </summary>
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {
        }

        public DbSet<ApiToken> ApiToken { get; set; }
        public DbSet<PasswordResetToken> PasswordResetToken { get; set; }

        public DbSet<IdentityUser> IdentityUser { get; set; }
    }
}
