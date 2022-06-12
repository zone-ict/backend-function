using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;

namespace Com.ZoneIct
{
    public class LineClient
    {
        const string BlockName = "ブロックユーザー";
        static HttpClient _client = new HttpClient();
        public static async Task<string> ReplyMessage(State state, Message message)
        {
            return await ReplyMessage(state, new Message[] { message });
        }

        public static async Task<string> ReplyMessage(State state, Message[] messages)
        {
            var lm = new LineMessage();
            lm.ReplyToken = state.ReplyToken;
            lm.Messages = messages;
            return await CallLineApi(state, lm, System.Net.Http.HttpMethod.Post, "https://api.line.me/v2/bot/message/reply");
        }

        public static async Task<string> ReplyMessage(State state, string message)
        {
            return await ReplyMessage(state, new string[] { message });
        }

        public static async Task<string> ReplyMessage(State state, string[] messages)
        {
            var list = new List<Message>();
            foreach (var message in messages)
                list.Add(new Message { Text = message });
            return await ReplyMessage(state, list.ToArray());
        }

        public static async Task<string> PushMessage(State state, Message message, string toLineId = null)
        {
            return await PushMessage(state, new Message[] { message }, toLineId);
        }

        public static async Task<string> PushMessage(State state, Message[] messages, string toLineId = null)
        {
            var lm = new LineMessage();
            lm.Messages = messages;
            lm.To = toLineId ?? state.LineId;
            return await CallLineApi(state, lm, System.Net.Http.HttpMethod.Post, "https://api.line.me/v2/bot/message/push");
        }

        public static async Task<string> PushMessage(State state, string message, string toLineId = null)
        {
            return await PushMessage(state, new string[] { message }, toLineId);
        }

        public static async Task<string> PushMessage(State state, string[] messages, string toLineId = null)
        {
            var list = new List<Message>();
            foreach (var message in messages)
                list.Add(new Message { Text = message });
            return await PushMessage(state, list.ToArray(), toLineId);
        }

        static async Task<string> CallLineApi(State state, object postData, HttpMethod httpMethod, string url)
        {
            var req = new HttpRequestMessage(httpMethod, url);
            var token = System.Environment.GetEnvironmentVariable("CHANNEL_ACCESS_TOKEN");

            req.Headers.Add(@"Authorization", @"Bearer {" + token + "}");

            if (httpMethod == HttpMethod.Post)
            {
                if (postData is LineMessage)
                {
                    foreach (var rec in ((LineMessage)postData)?.Messages)
                    {
                        rec.QuickReply = state.QuickReply;
                        rec.Sender = state.Sender;
                    }
                }
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var json = JsonConvert.SerializeObject(postData, Formatting.Indented, settings);
                state.Logger.LogInformation($"request = {json}");
                req.Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await Policy.Handle<HttpRequestException>()
                    .Or<SocketException>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 1))
                    .ExecuteAsync(() => _client.SendAsync(req.Clone()));
                var body = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new HttpRequestException($"{response.StatusCode.ToString()} {body}");
                state.Logger.LogInformation($"response = {body}");
                return body;
            }
            catch (Exception e)
            {
                state.Logger.LogInformation(e.ToString());
                if (e.Message.ToLower().Contains("sender name contain ng words"))
                {
                    if (!string.IsNullOrEmpty(state.Sender.Name))
                        state.Sender.Name = String.Join(' ', state.Sender.Name.ToCharArray());
                    state.Logger.LogWarning($"sender name contain ng words = {state.Sender.Name}");
                    return await CallLineApi(state, postData, httpMethod, url);
                }
                else if (e.Message.ToLower().Contains("invalid reply token"))
                {
                    try
                    {
                        await LineClient.PushMessage(state, new Message[] { new Message($"送信に失敗しました。再送をしてください。\n\n>> {state.Text}") });
                    }
                    catch { }
                    return null;
                }
                else if (e.Message.ToLower().Contains("not found"))
                    throw new UserNotFoundException();
                throw e;
            }
        }
        public static async Task<LineProfile> GetUserProfile(State state, string lineId)
        {
            try
            {
                var res = await CallLineApi(state, null, HttpMethod.Get, $"https://api.line.me/v2/bot/profile/{lineId}");
                dynamic profile = JsonConvert.DeserializeObject(res);
                return new LineProfile { Name = profile.displayName, Url = profile.pictureUrl, Language = profile.language };
            }
            catch (UserNotFoundException)
            {
                return new LineProfile { Name = BlockName };
            }
        }
        public static async Task MarkAsRead(State state, string lineId)
        {
            dynamic data = new { chat = new { userId = lineId } };
            await CallLineApi(state, data, HttpMethod.Post, "https://api.line.me/v2/bot/message/markAsRead");
        }
    }
    public class UserNotFoundException : Exception { }
}
