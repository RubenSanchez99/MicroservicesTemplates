using EventFlow.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventFlowService.Application.Infrastructure.AggregatesContext
{
    public class AggregatesContext : DbContext
    {
        public AggregatesContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEventFlowEvents();
            modelBuilder.AddEventFlowSnapshots();
        }
    }
}
