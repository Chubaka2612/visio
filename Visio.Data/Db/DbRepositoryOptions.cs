using Microsoft.Azure.Cosmos;

namespace Visio.Data.Core.Db
{
    public class RepositoryOptions
    {
        public string DatabaseId { get; set; }

        public CosmosClient CosmosClient { get; set; }

        public RepositoryOptions()
        {
        }

        public RepositoryOptions(string databaseId, CosmosClient cosmosClient)
        {
            DatabaseId = databaseId;
            CosmosClient = cosmosClient;
        }
    }
}
