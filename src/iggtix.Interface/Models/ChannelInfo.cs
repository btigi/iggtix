namespace iggtix.Interface.Models
{
    public class ChannelInfo
    {
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public string broadcaster_id { get; set; }
        public string broadcaster_login { get; set; }
        public string broadcaster_name { get; set; }
        public string broadcaster_language { get; set; }
        public string game_id { get; set; }
        public string game_name { get; set; }
        public string title { get; set; }
        public int delay { get; set; }
        public string[] tags { get; set; }
        public string[] content_classification_labels { get; set; }
        public bool is_branded_content { get; set; }
    }
}