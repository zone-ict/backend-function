using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
                    else if (state.Text == "Chat")
                    {
                        await StartChat(state);
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
            var businessSession = await CosmosClient<UserSession>.SingleOrDefaultAsync(x => x.id == Constants.BusinessId);
            if (state.IsBusinessId)
            {
                if (businessSession.talkLanguage == "ja")
                    await LineClient.PushMessage(state, state.Text, businessSession.talkId);
                else
                {
                    var translated = await CognitiveClient.Translate(state.Text, businessSession.talkLanguage);
                    await LineClient.PushMessage(state, new string[] { state.Text, translated }, businessSession.talkId);
                }
            }
            else
            {
                var reply = new ReplyButtonMessage(state.Session.name, state.LineId);
                if (businessSession.talkLanguage == "ja")
                {
                    if (businessSession.talkId != state.LineId)
                        await LineClient.PushMessage(state, new Message[] { new Message(state.Text), reply }, Constants.BusinessId);
                    else
                        await LineClient.PushMessage(state, state.Text, Constants.BusinessId);
                }
                else
                {
                    var translated = await CognitiveClient.Translate(state.Text, "ja");
                    if (businessSession.talkId != state.LineId)
                        await LineClient.PushMessage(state, new Message[] { new Message(state.Text), new Message(translated), reply }, Constants.BusinessId);
                    else
                        await LineClient.PushMessage(state, new string[] { state.Text, translated }, Constants.BusinessId);
                }
                businessSession.talkId = state.LineId;
                businessSession.talkLanguage = state.Session.language;
                await CosmosClient<UserSession>.UpsertDocumentAsync(businessSession);
            }
        }

        static async Task Postback(dynamic data, State state)
        {
            string postback = data.postback.data;
            var str = postback.Split('=');
            var user = await CosmosClient<UserSession>.SingleOrDefaultAsync(x => x.id == str[1]);

            if (user == null)
                await LineClient.ReplyMessage(state, "ユーザーが見つかりません。");
            else
            {
                switch (str[0])
                {
                    case "replyTo":
                        var me = await CosmosClient<UserSession>.SingleOrDefaultAsync(x => x.id == state.LineId);
                        me.talkId = user.id;
                        me.talkLanguage = user.language;
                        await CosmosClient<UserSession>.UpsertDocumentAsync(me);
                        await LineClient.ReplyMessage(state, "設定しました。");
                        break;
                }
            }
        }

        static async Task Follow(string type, State state)
        {
            await Task.Delay(0);
            if (type == "follow")
            {
                await StartChat(state);
            }
        }
        static async Task StartChat(State state)
        {
            var user = await LineClient.GetUserProfile(state, state.LineId);
            state.Session.language = user.Language;
            state.Session.name = user.Name;

            dynamic obj = JsonConvert.DeserializeObject(Lang.Code);
            var lang = obj[user.Language].name;
            var welcome = $"こんにちは\n{lang}でチャットができます!";
            if (user.Language == "ja")
                await LineClient.ReplyMessage(state, welcome);
            else
            {
                var translated = await CognitiveClient.Translate(welcome, user.Language);
                await LineClient.ReplyMessage(state, new string[] { welcome, translated });
            }
        }

        static async Task Read(dynamic data, State state)
        {
            await LineClient.PushMessage(state, "既読", data.readTo.ToString());
        }
    }
}
