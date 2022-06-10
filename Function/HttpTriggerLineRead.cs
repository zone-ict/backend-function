using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Com.ZoneIct
{
    public static class HttpTriggerLineRead
    {
        [FunctionName("HttpTriggerLineRead")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            string from = req.Query["readFrom"];
            string to = req.Query["readTo"];
            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                dynamic data = new { type = "read", lineId = from, readTo = to };

                QueueClient queue = new QueueClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "normal");
                await queue.CreateAsync();
                await queue.SendMessageAsync(JsonConvert.SerializeObject(data));
            }
            return new RedirectResult($"{Constants.ImagePath}icon_reply.png", true);
        }
    }
}
