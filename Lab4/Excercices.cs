using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace CosmosDBLab.Lab4
{
    public static class Excercices
    {
        private static readonly Uri _endpointUri = new Uri("");
        private static readonly string _primaryKey = "";
        private static readonly string _databaseId = "FinancialDatabase";
        private static readonly string _peopleCollectionId = "PeopleCollection";
        private static readonly string _transactionCollectionId = "TransactionCollection";

        public static async Task CreatePeopleCollection()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();

                var databaseLink = UriFactory.CreateDatabaseUri("FinancialDatabase");
                var peopleCollection = new DocumentCollection
                {
                    Id = _peopleCollectionId
                };
                var requestOptions = new RequestOptions
                {
                    OfferThroughput = 1000
                };

                peopleCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
                    databaseLink,
                    peopleCollection,
                    requestOptions);
                await Console.Out.WriteLineAsync($"People Collection Self-Link:\t{peopleCollection.SelfLink}");
            }
        }

        public static async Task CreateAPerson()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _peopleCollectionId);
                var doc = new Bogus.Person();
                var response = await client.CreateDocumentAsync(collectionLink, doc);
                await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
            }
        }

        public static async Task CreateAFamily()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _peopleCollectionId);
                var doc = new
                {
                    Person = new Bogus.Person(),
                    Relatives = new
                    {
                        Spouse = new Bogus.Person(),
                        Children = Enumerable.Range(0, 4).Select(r => new Bogus.Person())
                    }
                };
                var response = await client.CreateDocumentAsync(collectionLink, doc);
                await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
            }
        }

        public static async Task ReadAndFail()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var documentLink = UriFactory.CreateDocumentUri(_databaseId, _peopleCollectionId, "example.document");
                var doc = new
                {
                    id = "example.document",
                    FirstName = "Example",
                    LastName = "Person"
                };
                var readResponse = await client.ReadDocumentAsync(documentLink);
                await Console.Out.WriteLineAsync($"{readResponse.StatusCode}");
            }
        }

        public static async Task ReadAndHandleError()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var documentLink = UriFactory.CreateDocumentUri(_databaseId, _peopleCollectionId, "example.document");
                var doc = new
                {
                    id = "example.document",
                    FirstName = "Example",
                    LastName = "Person"
                };
                try
                {
                    var readResponse = await client.ReadDocumentAsync(documentLink);
                    await Console.Out.WriteLineAsync($"{readResponse.StatusCode}");
                }
                catch (DocumentClientException dex)
                {
                    await Console.Out.WriteLineAsync($"Exception: {dex.StatusCode}");
                }
            }
        }

        public static async Task Upsert()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _peopleCollectionId);                var doc = new
                {
                    id = "example.document",
                    FirstName = "Example",
                    LastName = "Person"
                };
                try
                {
                    var readResponse = await client.UpsertDocumentAsync(collectionLink, doc);
                    await Console.Out.WriteLineAsync($"{readResponse.StatusCode}");                }
                catch (DocumentClientException dex)
                {
                    await Console.Out.WriteLineAsync($"Exception: {dex.StatusCode}");
                }
            }
        }

        public static async Task CreateManyTransactions()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var transactions = new Bogus.Faker<Transaction>()
                    .RuleFor(
                        t => t.amount, 
                        fake => Math.Round(fake.Random.Double(5, 500), 2))
                    .RuleFor(
                        t => t.processed, 
                        fake => fake.Random.Bool(0.6f))
                    .RuleFor(
                        t => t.paidBy, 
                        fake => $"{fake.Name.FirstName().ToLower()}.{fake.Name.LastName().ToLower()}")
                    .RuleFor(
                        t => t.costCenter, 
                        fake => fake.Commerce.Department(1).ToLower())
                    .GenerateLazy(100);
                foreach (var transaction in transactions)
                {
                    var result = await client.CreateDocumentAsync(collectionLink, transaction);
                    await Console.Out.WriteLineAsync($"Document Created\t{result.Resource.Id}");
                }
            }
        }

        public static async Task CreateManyTransactionsAsync()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var transactions = new Bogus.Faker<Transaction>()
                    .RuleFor(
                        t => t.amount,
                        fake => Math.Round(fake.Random.Double(5, 500), 2))
                    .RuleFor(
                        t => t.processed,
                        fake => fake.Random.Bool(0.6f))
                    .RuleFor(
                        t => t.paidBy,
                        fake => $"{fake.Name.FirstName().ToLower()}.{fake.Name.LastName().ToLower()}")
                    .RuleFor(
                        t => t.costCenter,
                        fake => fake.Commerce.Department(1).ToLower())
                    .GenerateLazy(100);
                var tasks = new List<Task<ResourceResponse<Document>>>();
                foreach (var transaction in transactions)
                {
                    var resultTask = client.CreateDocumentAsync(collectionLink, transaction);
                    tasks.Add(resultTask);
                }
                Task.WaitAll(tasks.ToArray());
                foreach (var task in tasks)
                {
                    await Console.Out.WriteLineAsync($"Document Created\t{task.Result.Resource.Id}");
                }
            }
        }

        public static async Task CreateTooManyTransactionsAsync()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var transactions = new Bogus.Faker<Transaction>()
                    .RuleFor(
                        t => t.amount,
                        fake => Math.Round(fake.Random.Double(5, 500), 2))
                    .RuleFor(
                        t => t.processed,
                        fake => fake.Random.Bool(0.6f))
                    .RuleFor(
                        t => t.paidBy,
                        fake => $"{fake.Name.FirstName().ToLower()}.{fake.Name.LastName().ToLower()}")
                    .RuleFor(
                        t => t.costCenter,
                        fake => fake.Commerce.Department(1).ToLower())
                    .GenerateLazy(5000);
                var tasks = new List<Task<ResourceResponse<Document>>>();
                foreach (var transaction in transactions)
                {
                    var resultTask = client.CreateDocumentAsync(collectionLink, transaction);
                    tasks.Add(resultTask);
                }
                Task.WaitAll(tasks.ToArray());
                foreach (var task in tasks)
                {
                    await Console.Out.WriteLineAsync($"Document Created\t{task.Result.Resource.Id}");
                }
            }
        }

        public static async Task QueryMetrics()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    PopulateQueryMetrics = true
                };
                string sql = "SELECT TOP 1000 * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                var result = await query.ExecuteNextAsync();
                foreach (string key in result.QueryMetrics.Keys)
                {
                    await Console.Out.WriteLineAsync($"{key}\t{result.QueryMetrics[key]}");
                }
            }
        }

        public static async Task QueryMetrics2()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    PopulateQueryMetrics = true
                };
                string sql = "SELECT * FROM c WHERE c.processed = true"; 
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                var result = await query.ExecuteNextAsync();
                foreach (string key in result.QueryMetrics.Keys)
                {
                    await Console.Out.WriteLineAsync($"{key}\t{result.QueryMetrics[key]}");
                }
            }
        }

        public static async Task QueryMetrics3()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    PopulateQueryMetrics = true
                };
                string sql = "SELECT * FROM c";
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                var result = await query.ExecuteNextAsync();
                foreach (string key in result.QueryMetrics.Keys)
                {
                    await Console.Out.WriteLineAsync($"{key}\t{result.QueryMetrics[key]}");
                }
            }
        }

        public static async Task QueryMetrics4()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    PopulateQueryMetrics = true
                };
                string sql = "SELECT c.id FROM c";
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                var result = await query.ExecuteNextAsync();
                foreach (string key in result.QueryMetrics.Keys)
                {
                    await Console.Out.WriteLineAsync($"{key}\t{result.QueryMetrics[key]}");
                }
            }
        }

        public static async Task SlowRunningQuery()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var timer = new Stopwatch();
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    MaxItemCount = 100,
                    MaxDegreeOfParallelism = 1,
                    MaxBufferedItemCount = 0
                };
                await Console.Out.WriteLineAsync($"MaxItemCount:\t{options.MaxItemCount}");
                await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{options.MaxDegreeOfParallelism}");
                await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{options.MaxBufferedItemCount}");
                string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                timer.Start();
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    var result = await query.ExecuteNextAsync<Document>();
                }
                timer.Stop();
                await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
            }
        }

        public static async Task TunedQuery()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var timer = new Stopwatch();
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    MaxItemCount = 100,
                    MaxDegreeOfParallelism = 5,
                    MaxBufferedItemCount = 0
                };
                await Console.Out.WriteLineAsync($"MaxItemCount:\t{options.MaxItemCount}");
                await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{options.MaxDegreeOfParallelism}");
                await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{options.MaxBufferedItemCount}");
                string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                timer.Start();
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    var result = await query.ExecuteNextAsync<Document>();
                }
                timer.Stop();
                await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
            }
        }

        public static async Task TunedQuery2()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var timer = new Stopwatch();
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    MaxItemCount = 100,
                    MaxDegreeOfParallelism = 5,
                    MaxBufferedItemCount = -1
                };
                await Console.Out.WriteLineAsync($"MaxItemCount:\t{options.MaxItemCount}");
                await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{options.MaxDegreeOfParallelism}");
                await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{options.MaxBufferedItemCount}");
                string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                timer.Start();
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    var result = await query.ExecuteNextAsync<Document>();
                }
                timer.Stop();
                await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
            }
        }

        public static async Task TunedQuery3()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var timer = new Stopwatch();
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    MaxItemCount = 100,
                    MaxDegreeOfParallelism = -1,
                    MaxBufferedItemCount = -1
                };
                await Console.Out.WriteLineAsync($"MaxItemCount:\t{options.MaxItemCount}");
                await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{options.MaxDegreeOfParallelism}");
                await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{options.MaxBufferedItemCount}");
                string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                timer.Start();
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    var result = await query.ExecuteNextAsync<Document>();
                }
                timer.Stop();
                await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
            }
        }

        public static async Task TunedQuery4()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var timer = new Stopwatch();
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    MaxItemCount = 500,
                    MaxDegreeOfParallelism = -1,
                    MaxBufferedItemCount = -1
                };
                await Console.Out.WriteLineAsync($"MaxItemCount:\t{options.MaxItemCount}");
                await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{options.MaxDegreeOfParallelism}");
                await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{options.MaxBufferedItemCount}");
                string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                timer.Start();
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    var result = await query.ExecuteNextAsync<Document>();
                }
                timer.Stop();
                await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
            }
        }

        public static async Task TunedQuery5()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var timer = new Stopwatch();
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    MaxItemCount = 1000,
                    MaxDegreeOfParallelism = -1,
                    MaxBufferedItemCount = -1
                };
                await Console.Out.WriteLineAsync($"MaxItemCount:\t{options.MaxItemCount}");
                await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{options.MaxDegreeOfParallelism}");
                await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{options.MaxBufferedItemCount}");
                string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                timer.Start();
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    var result = await query.ExecuteNextAsync<Document>();
                }
                timer.Stop();
                await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
            }
        }

        public static async Task TunedQuery6()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _transactionCollectionId);
                var timer = new Stopwatch();
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true,
                    MaxItemCount = 1000,
                    MaxDegreeOfParallelism = -1,
                    MaxBufferedItemCount = 50000
                };
                await Console.Out.WriteLineAsync($"MaxItemCount:\t{options.MaxItemCount}");
                await Console.Out.WriteLineAsync($"MaxDegreeOfParallelism:\t{options.MaxDegreeOfParallelism}");
                await Console.Out.WriteLineAsync($"MaxBufferedItemCount:\t{options.MaxBufferedItemCount}");
                string sql = "SELECT * FROM c WHERE c.processed = true ORDER BY c.amount DESC";
                timer.Start();
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql, options).AsDocumentQuery();
                while (query.HasMoreResults)
                {
                    var result = await query.ExecuteNextAsync<Document>();
                }
                timer.Stop();
                await Console.Out.WriteLineAsync($"Elapsed Time:\t{timer.Elapsed.TotalSeconds}");
            }
        }

        public static async Task ReadTopDocument()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _peopleCollectionId);
                string sql = "SELECT TOP 1 * FROM c WHERE c.id = 'example.document'";
                var query = client.CreateDocumentQuery<Document>(collectionLink, sql).AsDocumentQuery();
                var response = await query.ExecuteNextAsync<Document>();
                await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
            }
        }

        public static async Task ReadSingleDocument()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var documentLink = UriFactory.CreateDocumentUri(_databaseId, _peopleCollectionId, "example.document");
                var response = await client.ReadDocumentAsync(documentLink);
                await Console.Out.WriteLineAsync($"{response.RequestCharge} RUs");
            }
        }

        public static async Task QueryAndShowETag()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var documentLink = UriFactory.CreateDocumentUri(_databaseId, _peopleCollectionId, "example.document");
                var response = await client.ReadDocumentAsync(documentLink);
                await Console.Out.WriteLineAsync($"ETag: {response.Resource.ETag}");
            }
        }

        public static async Task QueryAndUpdateAndShowETag()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var documentLink = UriFactory.CreateDocumentUri(_databaseId, _peopleCollectionId, "example.document");
                var response = await client.ReadDocumentAsync(documentLink);
                await Console.Out.WriteLineAsync($"Existing ETag:\t{response.Resource.ETag}");
                var cond = new AccessCondition { Condition = response.Resource.ETag, Type = AccessConditionType.IfMatch };
                response.Resource.SetPropertyValue("FirstName", "Demo");
                var options = new RequestOptions { AccessCondition = cond };
                response = await client.ReplaceDocumentAsync(response.Resource, options);
                await Console.Out.WriteLineAsync($"New ETag:\t{response.Resource.ETag}");
            }
        }
    }
}