namespace API.Model
{
    public class CsvMapping
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string CsvColumnName { get; set; }
        public string AuxColumnName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int IndexCsvColumn { get; set; }
    }
}
