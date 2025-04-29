namespace API.Model
{
    public class CsvRequest
    {
        public string FileName { get; set; }
        public List<Dictionary<string, string>> data { get; set; }
  
    }
}
