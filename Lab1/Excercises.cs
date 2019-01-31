using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDBLab.Lab1
{
    public static class Excercices
    {
        private static readonly Uri _endpointUri = new Uri("");
        private static readonly string _primaryKey = "";

        public static async Task CreateADatabase()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var targetDatabase = new Database
                {
                    Id = "EntertainmentDatabase"
                };

                targetDatabase = await client.CreateDatabaseIfNotExistsAsync(targetDatabase);
                await Console.Out.WriteLineAsync($"Database Self-Link:\t{targetDatabase.SelfLink}");
            }
        }

        public static async Task CreateAFixecCollection()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();

                var databaseLink = UriFactory.CreateDatabaseUri("EntertainmentDatabase");
                var defaultCollection = new DocumentCollection
                {
                    Id = "DefaultCollection"
                };

                defaultCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
                    databaseLink,
                    defaultCollection);
                await Console.Out.WriteLineAsync($"Default Collection Self-Link:\t{defaultCollection.SelfLink}");
            }
        }

        public static async Task CreateAnUlimitedCollection()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();

                var databaseLink = UriFactory.CreateDatabaseUri("EntertainmentDatabase");
                var indexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true,
                    IncludedPaths = new Collection<IncludedPath>
                    {
                        new IncludedPath
                        {
                            Path = "/*",
                            Indexes = new Collection<Index>
                            {
                                new RangeIndex(DataType.Number, -1),
                                new RangeIndex(DataType.String, -1)
                            }
                        }
                    }
                };

                var partitionKeyDefinition = new PartitionKeyDefinition
                {
                    Paths = new Collection<string> { "/type" }
                };

                var customCollection = new DocumentCollection
                {
                    Id = "CustomCollection",
                    PartitionKey = partitionKeyDefinition,
                    IndexingPolicy = indexingPolicy
                };

                var requestOptions = new RequestOptions
                {
                    OfferThroughput = 10000
                };

                customCollection = await client.CreateDocumentCollectionIfNotExistsAsync(
                    databaseLink,
                    customCollection,
                    requestOptions);
                await Console.Out.WriteLineAsync($"Custom Collection Self-Link:\t{customCollection.SelfLink}");
            }
        }

        public static async Task PopulateCollection()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(
                    "EntertainmentDatabase",
                    "CustomCollection");

                var foodInteractions = new Bogus.Faker<PurchaseFoodOrBeverage>()
                    .RuleFor(i => i.type, (fake) => nameof(PurchaseFoodOrBeverage))
                    .RuleFor(i => i.unitPrice, (fake) => Math.Round(fake.Random.Decimal(1.99m, 15.99m), 2))
                    .RuleFor(i => i.quantity, (fake) => fake.Random.Number(1, 5))
                    .RuleFor(i => i.totalPrice, (fake, user) => Math.Round(user.unitPrice * user.quantity, 2))
                    .Generate(500);
                foreach (var interaction in foodInteractions)
                {
                    var result = await client.CreateDocumentAsync(collectionLink, interaction);
                    await Console.Out.WriteLineAsync(
                        $"Document #{foodInteractions.IndexOf(interaction):000} Created\t{result.Resource.Id}");
                }
            }
        }

        public static async Task PopulateCollectionWithDifferentDataType()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(
                    "EntertainmentDatabase",
                    "CustomCollection");

                var tvInteractions = new Bogus.Faker<WatchLiveTelevisionChannel>()
                    .RuleFor(i => i.type, (fake) => nameof(WatchLiveTelevisionChannel))
                    .RuleFor(i => i.minutesViewed, (fake) => fake.Random.Number(1, 45))
                    .RuleFor(i => i.channelName, (fake) => fake.PickRandom(new List<string> { "NEWS-6", "DRAMA-15", "ACTION-12", "DOCUMENTARY-4", "SPORTS-8" }))
                    .Generate(500);
                foreach (var interaction in tvInteractions)
                {
                    var result = await client.CreateDocumentAsync(collectionLink, interaction);
                    await Console.Out.WriteLineAsync(
                        $"Document #{tvInteractions.IndexOf(interaction):000} Created\t{result.Resource.Id}");
                }
            }
        }

        public static async Task PopulateCollectionWithAnotherDifferentDataType()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(
                    "EntertainmentDatabase",
                    "CustomCollection");

                var mapInteractions = new Bogus.Faker<ViewMap>()
                    .RuleFor(i => i.type, (fake) => nameof(ViewMap))
                    .RuleFor(i => i.minutesViewed, (fake) => fake.Random.Number(1, 45))
                    .Generate(500);
                foreach (var interaction in mapInteractions)
                {
                    var result = await client.CreateDocumentAsync(collectionLink, interaction);
                    await Console.Out.WriteLineAsync(
                        $"Document #{mapInteractions.IndexOf(interaction):000} Created\t{result.Resource.Id}");
                }
            }
        }
    }
}