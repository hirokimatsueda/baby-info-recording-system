using Azure.Identity;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace BlazorApp.Api
{
    public static class PoopFunction
    {
        private static Lazy<CosmosClient> lazyClient = new Lazy<CosmosClient>(InitializeCosmosClient);
        private static CosmosClient cosmosClient => lazyClient.Value;

        private static CosmosClient InitializeCosmosClient()
        {
            return new CosmosClient(Environment.GetEnvironmentVariable("COSMOS_ENDPOINT", EnvironmentVariableTarget.Process), new DefaultAzureCredential());
        }

        /// <summary>
        /// 指定した月のうんちの一覧取得
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetPoops")]
        public static async Task<IActionResult> GetAsync(
           [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req, ILogger log)
        {
            log.LogInformation("GetPoops function processed a request.");

            if (!req.Query.TryGetValue("userId", out var userId))
            {
                return new BadRequestObjectResult("Parameter 'userId' is required");
            }
            if (!req.Query.TryGetValue("year", out var yearValue))
            {
                return new BadRequestObjectResult("Parameter 'year' is required");
            }
            if (!int.TryParse(yearValue, out int year))
            {
                return new BadRequestObjectResult("Parameter 'year' is not a number");
            }
            if (!req.Query.TryGetValue("month", out var monthValue))
            {
                return new BadRequestObjectResult("Parameter 'month' is required");
            }
            if (!int.TryParse(monthValue, out int month))
            {
                return new BadRequestObjectResult("Parameter 'month' is not a number");
            }
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var container = cosmosClient.GetContainer("db", "poops");

            try
            {
                var iterator = container.GetItemLinqQueryable<Poop>()
                    .Where(p => p.UserId == userId[0])
                    .Where(p => startDate <= p.Date && p.Date < endDate)
                    .ToFeedIterator();
                var poopList = new List<Poop>();
                do
                {
                    poopList.AddRange(await iterator.ReadNextAsync());

                } while (iterator.HasMoreResults);
                return new OkObjectResult(poopList);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new InternalServerErrorResult();
            }
        }

        /// <summary>
        /// 指定した日のうんちの更新
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("PutPoop")]
        public static async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequest req, ILogger log)
        {
            log.LogInformation("AddPoop function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var addPoopRequest = JsonConvert.DeserializeObject<PutPoopRequest>(requestBody);
            
            var container = cosmosClient.GetContainer("db", "poops");

            try
            {
                // うんち記録を更新する
                var poop = new Poop
                {
                    Id = addPoopRequest.GetId(),
                    UserId = addPoopRequest.UserId,
                    Date = addPoopRequest.Date,
                    Count = addPoopRequest.Count
                };
                PartitionKey partitionKey = new PartitionKey(addPoopRequest.UserId.ToString());
                var currentPoop = await container.UpsertItemAsync<Poop>(poop, partitionKey);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new InternalServerErrorResult();
            }

            return new OkResult();
        }
    }
}
