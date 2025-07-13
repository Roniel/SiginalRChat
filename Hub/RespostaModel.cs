namespace SiginalRChat.Hubs
{

    public class RespostaModel
    {
        public string model { get; set; }
        public string created_at { get; set; }
        public string done_reason { get; set; }
        public string response { get; set; }
        public bool? done { get; set; }
        public double? total_duration { get; set; }
        public double? load_duration { get; set; }
        public int? prompt_eval_count { get; set; }
        public double? prompt_eval_duration { get; set; }
        public int? eval_count { get; set; }
        public double? eval_duration { get; set; }
        
    }
}
 