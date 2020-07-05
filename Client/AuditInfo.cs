namespace Client
{
    public class AuditInfo
    {
        public string Message { get; set; }
        public string Reason { get; set; }
        public bool DontSendNotification { get; set; }
    }
}
