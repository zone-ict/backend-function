using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Com.ZoneIct
{
    public static class QueueTriggerMessageError
    {
        [FunctionName("QueueTriggerMessageError")]
        public static async Task Run(
            [QueueTrigger("message-poison", Connection = "AzureWebJobsStorage")] dynamic data,
            ILogger log)
        {
            #region initialize
            string type = data.type;

            var state = new State
            {
                Text = data.message?.text,
                ReplyToken = data.replyToken,
                LineId = data.lineId,
                Logger = log
            };
            #endregion

            state.Logger.LogInformation($"error input = {data.ToString()}");

            switch (type)
            {
                case "message":
                case "read":
                    break;
                default:
                    state.Logger.LogError($"invalid param : type={type}, Text={state.Text}, LineId={state.LineId}");
                    return;
            }
            var message = $"メッセージ送信に失敗しました。内容を確認して再送してください。\n\n>> {state.Text}";
            await LineClient.PushMessage(state, new Message[] { new Message(message) }, state.LineId);
        }
    }
}
