using Microsoft.Extensions.Logging;

namespace Com.ZoneIct
{
    public class State
    {
        public string LineId { get; set; }
        public string Text { get; set; }
        public Sender Sender { get; set; } = new BotSender();
        public QuickReply QuickReply { get; set; }
        public string ReplyToken { get; set; }
        public UserSession Session { get; set; }
        public ILogger Logger { get; set; }
        public bool IsBusinessId => Constants.BusinessId == LineId;
    }
}
