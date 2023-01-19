using Contoso.Healthcare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Contoso.Healthcare.Services
{
    public class PatientCosmosDbService : ICosmosDbService<Patient>
    {
        private Container _container;
        public PatientCosmosDbService(
            CosmosClient cosmosDbClient,
            string databaseName,
            string containerName)
        {
            _container = cosmosDbClient.GetContainer(databaseName, containerName);
        }
        public async Task<ItemResponse<Patient>> AddAsync(Patient item)
        {
            return await _container.CreateItemAsync(item, new PartitionKey(item.PatientId));
        }
        public async Task<ItemResponse<Patient>> DeleteAsync(string id, string patientId)
        {
            return await _container.DeleteItemAsync<Patient>(id, new PartitionKey(patientId));
        }
        public async Task<Patient> GetAsync(string id, string patientId)
        {
            try
            {
                var response = await _container.ReadItemAsync<Patient>(id, new PartitionKey(patientId));
                return response.Resource;
            }
            catch (CosmosException) //For handling item not found and other exceptions
            {
                return null;
            }
        }
        public async Task<IEnumerable<Patient>> GetMultipleAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<Patient>(new QueryDefinition(queryString));
            var results = new List<Patient>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }
        public async Task<ItemResponse<Patient>> UpdateAsync(string id, Patient item)
        {
            return await _container.UpsertItemAsync(item, new PartitionKey(item.PatientId));
        }
    }
}
