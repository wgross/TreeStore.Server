using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TreeStore.LiteDb;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using TreeStore.Model.Abstractions.Json;
using TreeStore.Server.Host.Middleware;

namespace TreeStore.Server.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)  
        {
            services
                .AddControllers()
                // Add Controllers from this assembly explicitly because during test the test assembly would be
                // searched for Controllers without success
                .AddApplicationPart(typeof(Startup).Assembly)
                .AddJsonOptions(options => TreeStoreJsonSerializerOptions.Apply(options.JsonSerializerOptions));

            services.AddCors(static options => options.AddPolicy(name: "localhost-only", static builder => builder.WithOrigins("http://localhost")));

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "TreeStore.Server.Host", Version = "v1" }));

            this.ConfigureTreeStoreServices(services);
        }

        protected virtual void ConfigureTreeStoreServices(IServiceCollection services)
        {
            services.Configure<TreeStoreLiteDbOptions>(this.Configuration.GetSection("LiteDb"));
            services.AddSingleton<ITreeStoreModel, TreeStoreLiteDbPersistence>();
            services.AddScoped<ITreeStoreService, TreeStoreService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TreeStore.Server.Host v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("localhost-only");

            app.UseAuthorization();

            app.UseMiddleware<MapExceptionMiddleware>();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}