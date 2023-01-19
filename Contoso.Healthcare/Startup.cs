using Contoso.Healthcare.Models;
using Contoso.Healthcare.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Threading.Tasks;

namespace Contoso.Healthcare
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contoso.Healthcare", Version = "v1" });
            });
            services.AddSingleton<ICosmosDbService<HealthCheck>>(InitializeHealthCheckService().GetAwaiter().GetResult());
            services.AddSingleton<ICosmosDbService<Patient>>(InitializePatientService().GetAwaiter().GetResult());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contoso.Healthcare v1"));

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static async Task<CosmosClient> InitializeCosmosClientInstanceAsync()
        {
            string databaseName = Environment.GetEnvironmentVariable("PowerAppsLabDatabaseName")!;
            string containerName = Environment.GetEnvironmentVariable("PowerAppsLabContainerName")!;
            string account = Environment.GetEnvironmentVariable("PowerAppsLabAccount")!;
            string key = Environment.GetEnvironmentVariable("PowerAppsLabKey")!;
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
            Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/PatientId");
            return client;
        }

        private static async Task<PatientCosmosDbService> InitializePatientService(){
            CosmosClient client = await InitializeCosmosClientInstanceAsync();
            return new PatientCosmosDbService(client,Environment.GetEnvironmentVariable("PowerAppsLabDatabaseName")!, Environment.GetEnvironmentVariable("PowerAppsLabContainerName")!);
        }

        private static async Task<HealthCheckCosmosDbService> InitializeHealthCheckService(){
            CosmosClient client = await InitializeCosmosClientInstanceAsync();
            return new HealthCheckCosmosDbService(client,Environment.GetEnvironmentVariable("PowerAppsLabDatabaseName")!, Environment.GetEnvironmentVariable("PowerAppsLabContainerName")!);
        }
    }
}
