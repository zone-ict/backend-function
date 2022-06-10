using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
    public static class HttpTriggerLineWebhook
    {
        [FunctionName("HttpTriggerLineWebhook")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            var secret = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("CHANNEL_SECRET"));
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var body = Encoding.UTF8.GetBytes(requestBody);
            var hmac = new HMACSHA256(secret);
            var hash = hmac.ComputeHash(body, 0, body.Length);
            var hash64 = Convert.ToBase64String(hash);

            var signature = req.Headers["X-Line-Signature"];
            if (signature != hash64)
                return new BadRequestObjectResult("Signature verification failed");

            log.LogInformation($"request = {requestBody}");
            dynamic records = JsonConvert.DeserializeObject(requestBody);
            foreach (dynamic data in records.events)
            {
                // add the specific values to the source
                data.lineId = data.source.userId;

                QueueClient queue = new QueueClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "normal");
                await queue.CreateAsync();
                await queue.SendMessageAsync(JsonConvert.SerializeObject(data));
            }
            return (ActionResult)new OkObjectResult(string.Empty);
        }
    }
}
