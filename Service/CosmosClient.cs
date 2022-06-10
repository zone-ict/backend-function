using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Polly;

namespace Com.ZoneIct
{
    public class CosmosClient<T>
    {
        static CosmosClient _client = new CosmosClient(Environment.GetEnvironmentVariable(Constants.CosmosDBConnection), new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Direct,
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            }
        });
        public static async Task UpsertDocumentAsync(T item)
        {
            dynamic rec = item;
            if (rec.id == null)
                rec.id = Guid.NewGuid().ToString();

            var container = GetContainer();
            await Policy.Handle<CosmosException>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 1))
                .ExecuteAsync(() => container.UpsertItemAsync<T>(item));
        }
        public static async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var container = GetContainer();
            int count = 0;
            T first = default(T);
            using (var iterator = container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator())
            {
                while (iterator.HasMoreResults)
                {
                    foreach (var item in await iterator.ReadNextAsync())
                    {
                        count++;
                        if (count > 1)
                            throw new InvalidOperationException($"SingleOrDefaultAsync more than 2 records {item.ToString()}");
                        first = item;
                    }
                    if (count >= 1)
                        return first;
                }
            }
            return default(T);
        }
        public static async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        {
            var container = GetContainer();
            var list = new List<T>();
            using (var iterator = container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator())
            {
                while (iterator.HasMoreResults)
                {
                    foreach (var item in await iterator.ReadNextAsync())
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }
        static Container GetContainer()
        {
            string container = typeof(T).Name.ToLower(); ;
            return _client.GetContainer(Constants.CosmosDB, container);
        }
    }
}
