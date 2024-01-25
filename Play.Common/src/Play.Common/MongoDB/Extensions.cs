using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;


namespace Play.Common.MongoDB
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            //mongodb serializer
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            // connecting to mongodb
            services.AddSingleton<IMongoClient>(serviceProvider => 
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetValue<string>("DatabaseSettings:ConnectionString");
                return new MongoClient(connectionString);
            });

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : IEntity
        {
            services.AddSingleton<IRespository<T>>(serviceProvider =>
            {
                var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
                return new MongoRepository<T>(mongoClient, collectionName);
            });

            return services;
        }
    }
}