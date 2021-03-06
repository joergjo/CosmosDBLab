using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace CosmosDBLab.Lab3
{
    public static class Excercices
    {
        private static readonly Uri _endpointUri = new Uri("");
        private static readonly string _primaryKey = "";

        public static async Task BulkUpload()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var sprocLink = UriFactory.CreateStoredProcedureUri(
                    "FinancialDatabase",
                    "InvestorCollection",
                    "bulkUpload");
                var people = new Faker<Person>()
                    .RuleFor(p => p.firstName, f => f.Name.FirstName())
                    .RuleFor(p => p.lastName, f => f.Name.LastName())
                    .RuleFor(p => p.company, f => "contosofinancial")
                    .Generate(25000);
                int pointer = 0;
                while (pointer < people.Count)
                {
                    var options = new RequestOptions { PartitionKey = new PartitionKey("contosofinancial") };
                    var result = await client.ExecuteStoredProcedureAsync<int>(
                        sprocLink,
                        options,
                        people.Skip(pointer));
                    pointer += result.Response;
                    await Console.Out.WriteLineAsync(
                        $"{pointer} Total Documents\t{result.Response} Documents Uploaded in this Iteration");
                }
            }
        }

        public static async Task BulkDelete()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var sprocLink = UriFactory.CreateStoredProcedureUri(
                    "FinancialDatabase",
                    "InvestorCollection",
                    "bulkDelete");
                bool resume = true;
                do
                {
                    var options = new RequestOptions { PartitionKey = new PartitionKey("contosofinancial") };
                    string query = "SELECT * FROM investors i WHERE i.company = 'contosofinancial'";
                    var result = await client.ExecuteStoredProcedureAsync<DeleteStatus>(sprocLink, options, query);
                    await Console.Out.WriteLineAsync(
                        $"Batch Delete Completed.\tDeleted: {result.Response.Deleted}\tContinue: {result.Response.Continuation}");
                    resume = result.Response.Continuation;
                }
                while (resume);
            }
        }
    }
}