using System.Threading.Tasks;
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
        [Queue("message"), StorageAccount("AzureWebJobsStorage")] IAsyncCollector<string> queue,
        ILogger log)
        {
            await Task.Delay(0);
            string from = req.Query["readFrom"];
            string to = req.Query["readTo"];
            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                dynamic data = new { type = "read", lineId = from, readTo = to };
                queue.AddAsync(JsonConvert.SerializeObject(data));
            }
            return new RedirectResult($"{Constants.ImagePath}icon_reply.png", true);
        }
    }
}
