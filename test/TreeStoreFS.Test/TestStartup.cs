using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TreeStore.LiteDb;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using TreeStore.Server.Host;

namespace TreeStoreFS.Test
{
    public class FileSystemTestStartup : Startup
    {
        private readonly ITreeStoreService service;

        public FileSystemTestStartup(IWebHostEnvironment hostingEnvironment, IConfiguration configuration) : base(hostingEnvironment, configuration)
        {
        }

        public FileSystemTestStartup(ITreeStoreService service, IWebHostEnvironment webHostEnvironment, IConfiguration configuration) : base(webHostEnvironment, configuration)
        {
            this.service = service;
        }

        protected override void ConfigureTreeStoreServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITreeStoreModel>(sc => TreeStoreLiteDbPersistence.InMemory(sc.GetRequiredService<ILoggerFactory>()));
            serviceCollection.AddScoped<ITreeStoreService, TreeStoreService>();
        }
    }
}