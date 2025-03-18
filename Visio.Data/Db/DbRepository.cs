using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Visio.Domain.Core;

namespace Visio.Data.Core.Db
{
    public abstract class Repository<TKey, TEntity> : IDbRepository<TKey, TEntity> where TEntity : Entity<TKey>, new()
    {
        protected readonly CosmosClient _cosmosClient;
        protected readonly Container _cosmosContainer;
        protected readonly ILogger<Repository<TKey, TEntity>> _logger;

        protected Repository(string tableName, RepositoryOptions entityDataStoreOptions, ILogger<Repository<TKey, TEntity>> logger)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            ArgumentNullException.ThrowIfNull(entityDataStoreOptions);
            ArgumentNullException.ThrowIfNull(entityDataStoreOptions.CosmosClient);
            ArgumentNullException.ThrowIfNull(entityDataStoreOptions.DatabaseId);
            ArgumentNullException.ThrowIfNull(logger);

            _cosmosClient = entityDataStoreOptions.CosmosClient;

            _cosmosContainer = _cosmosClient.GetDatabase(entityDataStoreOptions.DatabaseId).GetContainer(tableName);
            _logger = logger;
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                var response = await _cosmosContainer.CreateItemAsync(entity, new PartitionKey(entity.Id.ToString()));
                _logger.LogInformation("Entity added successfully with ID: {EntityId}", entity.Id);
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to add entity with ID: {EntityId}", entity.Id);
                throw;
            }
        }

        public async Task DeleteAsync(TKey id)
        {
            if (string.IsNullOrWhiteSpace(id.ToString()))
                throw new ArgumentNullException(nameof(id));

            try
            {
                var itemResponse = await _cosmosContainer.DeleteItemAsync<TEntity>(id.ToString(), new PartitionKey(id.ToString()));
                itemResponse.EnsureSuccessStatusCode();
                _logger.LogInformation("Entity with ID: {EntityId} deleted successfully", id);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to delete entity with ID: {EntityId}", id);
                throw;
            }
        }

        public async Task<TEntity> ReadAsync(TKey id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            try
            {
                var itemResponse = await _cosmosContainer.ReadItemAsync<TEntity>(id.ToString(), new PartitionKey(id.ToString()));
                itemResponse.EnsureSuccessStatusCode();
                _logger.LogInformation("Entity retrieved successfully with ID: {EntityId}", id);
                return itemResponse.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Entity with ID: {EntityId} not found", id);
                return null;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to retrieve entity with ID: {EntityId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> ReadAsync(QueryDefinition queryDefinition)
        {
            ArgumentNullException.ThrowIfNull(queryDefinition);

            var results = new List<TEntity>();

            try
            {
                var feedIterator = _cosmosContainer.GetItemQueryIterator<TEntity>(queryDefinition);
                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                _logger.LogInformation("Successfully retrieved {EntityCount} entities", results.Count);
                return results;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to retrieve entities");
                throw;
            }
        }

        public async Task UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                var itemResponse = await _cosmosContainer.ReplaceItemAsync(entity, entity.Id.ToString(), new PartitionKey(entity.Id.ToString()));
                itemResponse.EnsureSuccessStatusCode();
                _logger.LogInformation("Entity updated successfully with ID: {EntityId}", entity.Id);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to update entity with ID: {EntityId}", entity.Id);
                throw;
            }
        }
    }
}
