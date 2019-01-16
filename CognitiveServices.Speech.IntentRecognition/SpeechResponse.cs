namespace CognitiveServices.Speech.IntentRecognition
{
    public class SpeechResponse
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public Entity[] entities { get; set; }
    }

    public class TopScoringIntent
    {
        public string intent { get; set; }
        public decimal score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public Resolution resolution { get; set; }
    }

    public class Resolution
    {
        public string[] values { get; set; }
    }
}
