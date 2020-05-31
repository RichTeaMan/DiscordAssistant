using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant
{
    public class State
    {
        public DateTimeOffset LastUpdateDateTime { get; set; }

        public static State CreateDefault()
        {
            return new State
            {
                LastUpdateDateTime = DateTimeOffset.UtcNow
            };
        }
    }
}
