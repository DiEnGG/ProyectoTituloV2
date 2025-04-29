namespace WebApp.Models
{
    public class FileMapping
    {
        public string FileName { get; set; }
        public string CsvColumnName { get; set; }
        public string AuxColumnName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IndexCsvColumn { get; set; }
    }
}
