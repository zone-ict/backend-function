
namespace Com.ZoneIct
{
    public class LineMessage
    {
        public string To { get; set; }
        public string ReplyToken { get; set; }
        public Message[] Messages { get; set; }
        public string Destination { get; set; }
    }

    public class Message
    {
        public string Type { get; set; } = "text";
        public string AltText { get; set; }
        public string Text { get; set; }
        public Sender Sender { get; set; }
        public Template Template { get; set; }
        public Content Contents { get; set; }
        public QuickReply QuickReply { get; set; }

        public Message() { }
        public Message(string text)
        {
            Text = text;
        }
    }

    public class Content
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }
        public string AspectRatio { get; set; }
        public string AspectMode { get; set; }
        public string OffsetTop { get; set; }
        public string Layout { get; set; }
        public string Weight { get; set; }
        public string Align { get; set; }
        public string Margin { get; set; }
        public string Spacing { get; set; }
        public string BackgroundColor { get; set; }
        public string CornerRadius { get; set; }
        public string Color { get; set; }
        public Content Hero { get; set; }
        public Content Body { get; set; }
        public Content Footer { get; set; }
        public string Text { get; set; }
        public string Style { get; set; }
        public string Height { get; set; }
        public MessageAction Action { get; set; }
        public bool? Wrap { get; set; }
        public Content[] Contents { get; set; }
    }

    public class Sender
    {
        string _name;
        public string Name
        {
            get
            {
                if (_name?.Length >= 21)
                    return _name.Substring(0, 19) + "…";
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string IconUrl { get; set; }
    }

    public class BotSender : Sender
    {
        public BotSender()
        {
            IconUrl = "https://previews.123rf.com/images/nastyatrel/nastyatrel2006/nastyatrel200600035/149438272-flat-vector-illustration-chat-bot-icon-flat-line-style-vector-.jpg";
        }
    }

    public class Template
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public MessageAction[] Actions { get; set; }
        public Column[] Columns { get; set; }
        public string ImageAspectRatio { get; set; }
        public string ImageSize { get; set; }
    }
    public class Column
    {
        public string ThumbnailImageUrl { get; set; }
        public string ImageBackgroundColor { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public MessageAction DefaultAction { get; set; }
        public MessageAction[] Actions { get; set; }
    }
    public class MessageAction
    {
        public string Type { get; set; }
        public string Label { get; set; }
        public string Text { get; set; }
        public string Uri { get; set; }
        public string Data { get; set; }
    }

    public class QuickReply
    {
        public Item[] Items { get; set; }
    }

    public class Item
    {
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public MessageAction Action { get; set; }
    }

    public class LineProfile
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Language { get; set; }
    }
}
