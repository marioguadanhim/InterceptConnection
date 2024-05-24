using System.Collections.Concurrent;

namespace InterceptConnection.Library
{
    public static class TheUltimateEventManager
    {
        public static bool Enabled { get; set; } = false;
        public static ConcurrentQueue<string> messagesIn { get; set; }
        public static ConcurrentQueue<string> messagesOut { get; set; }
    }
}
