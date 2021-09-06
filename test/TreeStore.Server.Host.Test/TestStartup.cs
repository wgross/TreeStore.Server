using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TreeStore.Model.Abstractions;

namespace TreeStore.Server.Host.Test
{
    public class TestStartup : Startup
    {
        private readonly ITreeStoreService service;

        public TestStartup(ITreeStoreService service, IWebHostEnvironment webHostEnvironment, IConfiguration configuration) : base(webHostEnvironment, configuration)
        {
            this.service = service;
        }

        protected override void ConfigureTreeStoreServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ITreeStoreService>(_ => this.service);
        }
    }
}