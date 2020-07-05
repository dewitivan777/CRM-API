using System.Threading.Tasks;

namespace Client
{
    public interface INameResolver
    {
        /// <summary>
        /// Finds and calls a query service based on the entityType
        /// </summary>
        /// <param name="entityId">The Id of the entity being resolved</param>
        /// <param name="entityType">The required service. Support name with extension e.g. Region or RegionId</param>
        /// <returns></returns>
        Task<string> ResolveNameAsync(string entityId, string entityType);

        /// <summary>
        /// Finds and calls a query service based on the entityType
        /// </summary>
        /// <param name="entityId">The Id of the entity being resolved</param>
        /// <param name="entityType">The required service. Support name with extension e.g. Region or RegionId</param>
        /// <returns></returns>
        Task<string> ResolveNameAsync(int entityId, string entityType);

        /// <summary>
        /// Check the entity for known type id's and query relevant services
        /// </summary>
        /// <typeparam name="T">Enntity type</typeparam>
        /// <param name="entity">The entity with names to be resolved</param>
        /// <returns></returns>
        Task<T> ResolveNamesAsync<T>(T entity) where T : class;
    }
}
