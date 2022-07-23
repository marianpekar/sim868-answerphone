namespace Sim868
{
    public struct TextMessage
    {
        public int Slot { get; private set; }
        public DateTime DateTime { get; private set; }
        public string Sender { get; private set; }
        public string Text { get; private set; }

        public static TextMessage Parse(string message)
        {
            string[] headerTextSplit = message.Split('\n');
            string header = headerTextSplit[0];
            string text = headerTextSplit[1];
            string[] headerParts = header.Replace("\"", "").Split(',');

            return new TextMessage()
            {
                Slot = ExtractSlot(headerParts),
                DateTime = new DateTime(ExtractDate(headerParts).Ticks + ExtractTime(headerParts).Ticks),
                Sender = headerParts[2],
                Text = text
            };
        }

        private static int ExtractSlot(string[] headerParts)
        {
            return int.Parse(headerParts[0].Split(":")[1].Trim());
        }

        private static DateTime ExtractDate(string[] headerParts)
        {     
            string date = headerParts[4];
            string[] rrmmdd = date.Split('/');
            int year = int.Parse("20" + rrmmdd[0]);
            int month = int.Parse(rrmmdd[1]);
            int day = int.Parse(rrmmdd[2]);

            return new DateTime(year, month, day);
        }

        private static TimeOnly ExtractTime(string[] headerParts)
        {
            string[] hhmmss = headerParts[5].Split(':');
            int hours = int.Parse(hhmmss[0]);
            int minutes = int.Parse(hhmmss[1]);
            int seconds = int.Parse(hhmmss[2][0..hhmmss[2].IndexOf('+')]);

            return new TimeOnly(hours, minutes, seconds);
        }
    }
}
