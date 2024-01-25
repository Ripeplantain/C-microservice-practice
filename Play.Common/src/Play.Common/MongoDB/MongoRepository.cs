using System.Linq.Expressions;
using MongoDB.Driver;


namespace Play.Common.MongoDB
{ 
    public class MongoRepository<T> : IRespository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> dbCollection;
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public MongoRepository(IMongoClient mongoClient, string collectionName)
        {
            var database = mongoClient.GetDatabase("Catalog");
            dbCollection = database.GetCollection<T>(collectionName);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).ToListAsync();
        }

        public async Task<T?> GetAsync(Guid id)
        {
            var filter = filterBuilder.Eq(entity => entity.Id, id);
            return await dbCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task CreateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await dbCollection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var filter = filterBuilder.Eq(existingEntity => existingEntity.Id, entity.Id);
            await dbCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            var filter = filterBuilder.Eq(entity => entity.Id, id);
            await dbCollection.DeleteOneAsync(filter);
        }
    }
}