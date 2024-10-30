namespace appsvc_function_dev_cm_sche_dotnet001
{
    public class Backup
    {
        public List<string> JobOpportunities { get; set; }
        public DateTime CreateDate { get; set; }

        public Backup()
        {
            CreateDate = DateTime.Now;
            JobOpportunities = new List<string>();
        }
    }
}
