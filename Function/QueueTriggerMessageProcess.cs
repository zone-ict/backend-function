using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Com.ZoneIct
{
    public static class QueueTriggerMessageProcess
    {
        [FunctionName("QueueTriggerMessageProcess")]
        public static async Task Run(
            [QueueTrigger("message", Connection = "AzureWebJobsStorage")] dynamic data,
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
            state.Logger.LogInformation($"input = {data.ToString()}");
            state.Sender = new BotSender();

            var sessionId = $"{state.LineId}";
            state.Session = await CosmosClient<UserSession>.SingleOrDefaultAsync(x => x.id == sessionId);
            state.Session = state.Session ?? new UserSession { id = sessionId };

            if (!await IsIgnoreType(state, type) && !await DevUtil.ProcessMessage(state))
            {
                if (type == "read")
                    await Read(data, state);
                else if (type == "follow" || type == "unfollow")
                    await Follow(type, state);
                else if (type == "postback")
                    await Postback(data, state);
                else
                    await SendMessage(state);
            }
            state.Session.previous = state.Text;
            await CosmosClient<UserSession>.UpsertDocumentAsync(state.Session);
        }
        static async Task<bool> IsIgnoreType(State state, string type)
        {
            var noreply = "テキスト以外は送信できません。";
            switch (type)
            {
                case "follow":
                case "unfollow":
                case "postback":
                case "read":
                    break;

                case "message":
                    if (string.IsNullOrEmpty(state.Text))
                    {
                        await LineClient.ReplyMessage(state, noreply);
                        return true;
                    }
                    break;

                default:
                    await LineClient.ReplyMessage(state, noreply);
                    return true;
            }
            return false;
        }

        static async Task SendMessage(State state)
        {
            // if (state.Text == "abc")
            await LineClient.ReplyMessage(state, state.Text + "、だよ");
        }

        static async Task Postback(dynamic data, State state)
        {
            string postback = data.postback.data;
            var str = postback.Split(',');
            var user = await CosmosClient<UserSession>.SingleOrDefaultAsync(x => x.id == str[1]);

            if (user == null)
                await LineClient.ReplyMessage(state, "ユーザーが見つかりません。");
            else
            {
                switch (str[0])
                {
                    case "A":
                        break;
                    case "B":
                        break;
                }
            }
        }

        static async Task Follow(string type, State state)
        {
            await Task.Delay(0);
            if (type == "follow")
            {
                var user = await LineClient.GetUserProfile(state, state.LineId);
                state.Session.language = user.Language;
                var welcome = "こんにちは、メッセージを入力してください";
                if (user.Language != "ja")
                    welcome = await AzureClient.Translate(welcome, user.Language);
                await LineClient.ReplyMessage(state, welcome);
            }
        }

        static async Task Read(dynamic data, State state)
        {
            await LineClient.PushMessage(state, "既読", data.readTo.ToString());
        }
    }
}
