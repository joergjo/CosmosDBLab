using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace CosmosDBLab.Lab2
{
    public static class Excercices
    {
        private static readonly Uri _endpointUri = new Uri("");
        private static readonly string _primaryKey = "";
        private static readonly string _databaseId = "UniversityDatabase";
        private static readonly string _collectionId = "StudentCollection";

        public static async Task Query()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                string sql = "SELECT TOP 5 VALUE s.studentAlias FROM coll s WHERE s.enrollmentYear = 2018 ORDER BY s.studentAlias";
                var query = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql));
                foreach (string alias in query)
                {
                    await Console.Out.WriteLineAsync(alias);
                }
            }
        }

        public static async Task QueryIntraDocumentArray()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                string sql = "SELECT s.clubs FROM students s WHERE s.enrollmentYear = 2018";
                var query = client.CreateDocumentQuery<Student>(collectionLink, new SqlQuerySpec(sql));
                foreach (Student student in query)
                {

                    foreach (string club in student.Clubs)
                    {
                        await Console.Out.WriteLineAsync(club);
                    }
                }
            }
        }

        public static async Task QueryIntraDocumentArrayWithDetails()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                string sql = "SELECT s.firstName, s.lastName, s.clubs FROM students s WHERE s.enrollmentYear = 2018";
                var query = client.CreateDocumentQuery<Student>(collectionLink, new SqlQuerySpec(sql));
                foreach (var student in query)
                {
                    await Console.Out.WriteLineAsync($"{student.FirstName} {student.LastName}");
                    foreach (string club in student.Clubs)
                    {
                        await Console.Out.WriteLineAsync($"\t{club}");
                    }
                    await Console.Out.WriteLineAsync();
                }
            }
        }

        public static async Task QueryAndFlatten()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                string sql = "SELECT VALUE activity FROM students s JOIN activity IN s.clubs WHERE s.enrollmentYear = 2018";
                var query = client.CreateDocumentQuery<string>(collectionLink, new SqlQuerySpec(sql));
                foreach (string activity in query)
                {
                    await Console.Out.WriteLineAsync(activity);
                }
            }
        }

        public static async Task QueryAndProject()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                string sql = "SELECT VALUE { 'id': s.id, 'name': CONCAT(s.firstName, ' ', s.lastName), 'email': { 'home': s.homeEmailAddress, 'school': CONCAT(s.studentAlias, '@contoso.edu') } } FROM students s WHERE s.enrollmentYear = 2018";
                var query = client.CreateDocumentQuery<StudentProfile>(collectionLink, new SqlQuerySpec(sql));
                foreach (StudentProfile profile in query)
                {
                    await Console.Out.WriteLineAsync(
                        $"[{profile.Id}]\t{profile.Name,-20}\t{profile.Email.School,-50}\t{profile.Email.Home}");
                }
            }
        }

        public static async Task QueryAndPaginate()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                string sql = "SELECT VALUE { 'id': s.id, 'name': CONCAT(s.firstName, ' ', s.lastName), 'email': { 'home': s.homeEmailAddress, 'school': CONCAT(s.studentAlias, '@contoso.edu') } } FROM students s WHERE s.enrollmentYear = 2018";
                var query = client.CreateDocumentQuery<StudentProfile>(collectionLink, new SqlQuerySpec(sql), new FeedOptions { MaxItemCount = 100 }).AsDocumentQuery();
                int pageCount = 0;
                while (query.HasMoreResults)
                {
                    await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
                    foreach (StudentProfile profile in await query.ExecuteNextAsync())
                    {
                        await Console.Out.WriteLineAsync(
                            $"\t[{profile.Id}]\t{profile.Name,-20}\t{profile.Email.School,-50}\t{profile.Email.Home}");
                    }
                }
            }
        }

        public static async Task QuerySinglePartition()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var query = client
                    .CreateDocumentQuery<Student2>(
                        collectionLink,
                        new FeedOptions { PartitionKey = new PartitionKey(2016) })
                    .Where(student => student.projectedGraduationYear == 2020);
                foreach (var student in query)
                {
                    Console.Out.WriteLine(
                        $"Enrolled: {student.enrollmentYear}\tGraduation: {student.projectedGraduationYear}\t{student.studentAlias}");
                }
            }
        }

        public static async Task QueryAcrossPartitions()
        {
            using (var client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var query = client
                    .CreateDocumentQuery<Student2>(
                        collectionLink, 
                        new FeedOptions { EnableCrossPartitionQuery = true })
                    .Where(student => student.projectedGraduationYear == 2020);
                foreach (var student in query)
                {
                    Console.Out.WriteLine(
                        $"Enrolled: {student.enrollmentYear}\tGraduation: {student.projectedGraduationYear}\t{student.studentAlias}");
                }
            }
        }

        public static async Task QueryUsingContinuationToken()
        {
            using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                string continuationToken = string.Empty;
                do
                {
                    var options = new FeedOptions
                    {
                        EnableCrossPartitionQuery = true,
                        RequestContinuation = continuationToken
                    };
                    var query = client
                        .CreateDocumentQuery<Student2>(collectionLink, options)
                        .Where(student => student.age < 18)
                        .AsDocumentQuery();

                    var results = await query.ExecuteNextAsync<Student2>();
                    continuationToken = results.ResponseContinuation;

                    await Console.Out.WriteLineAsync($"ContinuationToken:\t{continuationToken}");
                    foreach (var result in results)
                    {
                        await Console.Out.WriteLineAsync(
                            $"[Age: {result.age}]\t{result.studentAlias}@consoto.edu");
                    }
                    await Console.Out.WriteLineAsync();
                }
                while (!string.IsNullOrEmpty(continuationToken));
            } 
        }

        public static async Task QueryAcrossPartitions2()
        {
            using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true
                };

                string sql = "SELECT * FROM students s WHERE s.academicStatus.suspension = true";
                var query = client
                    .CreateDocumentQuery<Student2>(collectionLink, sql, options)
                    .AsDocumentQuery();

                int pageCount = 0;
                while (query.HasMoreResults)
                {
                    await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
                    foreach (var result in await query.ExecuteNextAsync())
                    {
                        await Console.Out.WriteLineAsync(
                            $"Enrollment: {result.enrollmentYear}\tBalance: {result.financialData.tuitionBalance}\t{result.studentAlias}@consoto.edu");
                    }
                }
            }
        }

        public static async Task QueryAcrossPartitions3()
        {
            using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true
                };

                string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14000";
                var query = client
                    .CreateDocumentQuery<Student2>(collectionLink, sql, options)
                    .AsDocumentQuery();

                int pageCount = 0;
                while (query.HasMoreResults)
                {
                    await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
                    foreach (var result in await query.ExecuteNextAsync())
                    {
                        await Console.Out.WriteLineAsync(
                            $"Enrollment: {result.enrollmentYear}\tBalance: {result.financialData.tuitionBalance}\t{result.studentAlias}@consoto.edu");
                    }
                }
            }
        }

        public static async Task QueryAcrossPartitions4()
        {
            using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true
                };

                string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14950";
                var query = client
                    .CreateDocumentQuery<Student2>(collectionLink, sql, options)
                    .AsDocumentQuery();

                int pageCount = 0;
                while (query.HasMoreResults)
                {
                    await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
                    foreach (var result in await query.ExecuteNextAsync())
                    {
                        await Console.Out.WriteLineAsync(
                            $"Enrollment: {result.enrollmentYear}\tBalance: {result.financialData.tuitionBalance}\t{result.studentAlias}@consoto.edu");
                    }
                }
            }
        }

        public static async Task QueryAcrossPartitions5()
        {
            using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true
                };

                string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14996";
                var query = client
                    .CreateDocumentQuery<Student2>(collectionLink, sql, options)
                    .AsDocumentQuery();

                int pageCount = 0;
                while (query.HasMoreResults)
                {
                    await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
                    foreach (var result in await query.ExecuteNextAsync())
                    {
                        await Console.Out.WriteLineAsync(
                            $"Enrollment: {result.enrollmentYear}\tBalance: {result.financialData.tuitionBalance}\t{result.studentAlias}@consoto.edu");
                    }
                }
            }
        }

        public static async Task QueryAcrossPartitions6()
        {
            using (DocumentClient client = new DocumentClient(_endpointUri, _primaryKey))
            {
                await client.OpenAsync();
                var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                var options = new FeedOptions
                {
                    EnableCrossPartitionQuery = true
                };

                string sql = "SELECT * FROM students s WHERE s.financialData.tuitionBalance > 14998";
                var query = client
                    .CreateDocumentQuery<Student2>(collectionLink, sql, options)
                    .AsDocumentQuery();

                int pageCount = 0;
                while (query.HasMoreResults)
                {
                    await Console.Out.WriteLineAsync($"---Page #{++pageCount:0000}---");
                    foreach (var result in await query.ExecuteNextAsync())
                    {
                        await Console.Out.WriteLineAsync(
                            $"Enrollment: {result.enrollmentYear}\tBalance: {result.financialData.tuitionBalance}\t{result.studentAlias}@consoto.edu");
                    }
                }
            }
        }
    }
}