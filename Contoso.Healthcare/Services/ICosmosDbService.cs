using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Contoso.Healthcare.Services
{
    public interface ICosmosDbService<T>
    {
        Task<IEnumerable<T>> GetMultipleAsync(string query);
        Task<T> GetAsync(string id, string patientId);
        Task<ItemResponse<T>> AddAsync(T item);
        Task<ItemResponse<T>> UpdateAsync(string id, T item);
        Task<ItemResponse<T>> DeleteAsync(string id, string patientId);
    }
}
