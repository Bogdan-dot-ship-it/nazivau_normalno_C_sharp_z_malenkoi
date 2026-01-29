namespace Core
{
    public class WorkReport
    {
        public int ReportId { get; set; }
        public int OrderId { get; set; }
        public string ReportContent { get; set; } = string.Empty;
    }
}
