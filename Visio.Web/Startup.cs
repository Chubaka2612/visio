using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Visio.Data.Core;
using Azure.Storage.Blobs;
using Visio.Services.ImageService;
using Visio.Data.Domain.Images;
using Visio.Data.Core.Storage;
using Visio.Data.Core.Db;
using Visio.Services.Notifications;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace Visio.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cosmosConfig = Configuration.GetSection("AzureCosmosOptions");
            var connectionString = cosmosConfig["ConnectionString"];
            var databaseId = cosmosConfig["DatabaseId"];

            var blobConfig = Configuration.GetSection("AzureBlobStorageOptions");
            var blobConnectionString = blobConfig["ConnectionString"];
            var containerId = blobConfig["ContainerId"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(databaseId))
            {
                throw new InvalidOperationException("Azure Cosmos DB configuration is missing in appsettings.json");
            }

            if (string.IsNullOrWhiteSpace(blobConnectionString) || string.IsNullOrWhiteSpace(containerId))
            {
                throw new InvalidOperationException("Azure Blob Storage configuration is missing in appsettings.json");
            }

            var cosmosClient = new CosmosClientBuilder(connectionString)
                .WithConnectionModeDirect()
                .Build();

            var blobServiceClient = new BlobServiceClient(blobConnectionString);
            var storageOptions = new StorageOptions(containerId, blobServiceClient);

            // Register StorageService as a singleton
            services.AddSingleton(storageOptions);
            services.AddSingleton<IStorageService, StorageService>();

            services.AddSingleton<CosmosClient>(cosmosClient);
            services.AddSingleton(new RepositoryOptions
            {
                CosmosClient = cosmosClient,
                DatabaseId = databaseId
            });

            // Register ImageRepository in Dependency Injection
            services.AddScoped<IImageRepository, ImageDbRepository>();
            services.AddScoped<IImageService, ImageService>();

            var busConfig = Configuration.GetSection("ServiceBusOptions");
            var busConnectionString = busConfig["ConnectionString"];
            var queueId = busConfig["QueueId"];
            services.AddSingleton(new ServiceBusOptions
            {
                ConnectionString = busConnectionString,
                QueueName = queueId
            });
          
            services.AddSingleton<INotificationProducer, NotificationProducer>();

            // Logging info
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Startup>();
        
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IServiceProvider serviceProvider)
        {

            // Debugging: Fetch Images from CosmosDB 
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IImageService>();

                Task.Run(async () =>
                {
                    using var fileStream = File.OpenRead("s3_test_photo.jpg");
                    var filemeta = new FileMetadata
                    {
                        FileName = "s3_test_photo.jpg",
                        Size = fileStream.Length
                    };

                    var addedImage = await service.CreateAsync(fileStream, filemeta);


                }).Wait();
            }

            app.UseStaticFiles();
            app.UseRouting();

            // Log application startup
            logger.LogInformation("Application starting up in {Environment} mode", env.EnvironmentName);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }
    }
}
