using EventFlow.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlowService.ReadModel.ReadModelContext
{
    public class ReadModelContextProvider : IDbContextProvider<ReadModelContext>, IDisposable
    {
        private readonly IServiceProvider provider;

        public ReadModelContextProvider(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public ReadModelContext CreateContext()
        {
            return provider.GetService<ReadModelContext>();
        }

        public void Dispose()
        {
        }
    }
}
