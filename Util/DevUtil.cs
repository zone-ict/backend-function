using System;
using System.Threading.Tasks;

namespace Com.ZoneIct
{
    public class DevUtil
    {
        public static async Task<bool> ProcessMessage(State state)
        {
            switch (state?.Text)
            {
                case "my":
                    await LineClient.ReplyMessage(state, state.LineId);
                    return true;

                case "ver":
                    await LineClient.ReplyMessage(state, Constants.Version);
                    return true;

                case "error":
                    throw new Exception("test error");
            }
            return false;
        }
    }
}
