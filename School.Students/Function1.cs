using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace School.Students
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var config = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("local.settings.json").Build();

                var CosmosDBEndpoint = config["Values:CosmosDBEndpoint"];
                var CosmosDBKey = config["Values:CosmosDBKey"];
                var databaseId = "SchoolDB";
                var containerId = "StudentContainer";
                List<string> studentNames = new List<string>();

                using (var client = new CosmosClient(CosmosDBEndpoint, CosmosDBKey))
                {
                    var database = client.GetDatabase(databaseId);
                    var container = database.GetContainer(containerId);

                    var stuQueryDef = new QueryDefinition("SELECT * FROM c");
                    var resultSet = container.GetItemQueryIterator<Student>(stuQueryDef);

                    while (resultSet.HasMoreResults)
                    {
                        foreach (var item in await resultSet.ReadNextAsync())
                        {
                            studentNames.Add(item.Name);
                        }
                    }

                    return new OkObjectResult(string.Join(", ", studentNames));

                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}
