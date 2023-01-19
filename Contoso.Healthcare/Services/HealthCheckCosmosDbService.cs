using Contoso.Healthcare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Contoso.Healthcare.Services
{
    public class HealthCheckCosmosDbService : ICosmosDbService<HealthCheck>
    {
        private Container _container;
        public HealthCheckCosmosDbService(
            CosmosClient cosmosDbClient,
            string databaseName,
            string containerName)
        {
            _container = cosmosDbClient.GetContainer(databaseName, containerName);
        }
        public async Task<ItemResponse<HealthCheck>> AddAsync(HealthCheck item)
        {
           return await _container.CreateItemAsync(item, new PartitionKey(item.PatientId));
        }
        public async Task<ItemResponse<HealthCheck>> DeleteAsync(string id, string patientId)
        {
            return await _container.DeleteItemAsync<HealthCheck>(id, new PartitionKey(patientId));
        }
        public async Task<HealthCheck> GetAsync(string id, string patientId)
        {
            try
            {
                var response = await _container.ReadItemAsync<HealthCheck>(id, new PartitionKey(patientId));
                return response.Resource;
            }
            catch (CosmosException) //For handling item not found and other exceptions
            {
                return null;
            }
        }
        public async Task<IEnumerable<HealthCheck>> GetMultipleAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<HealthCheck>(new QueryDefinition(queryString));
            var results = new List<HealthCheck>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }
        public async Task<ItemResponse<HealthCheck>> UpdateAsync(string id, HealthCheck item)
        {
            return await _container.UpsertItemAsync(item, new PartitionKey(item.PatientId));
        }
    }
}
