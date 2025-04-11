namespace API.Model
{
    public class CsvRequest
    {
        public string FileName { get; set; }
        public List<Dictionary<string, string>> Columnas { get; set; }
  
    }
}
