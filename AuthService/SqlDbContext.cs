using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
