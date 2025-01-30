namespace appsvc_function_dev_cm_sche_dotnet001
{
    internal class Globals
    {
        public static readonly string azureWebJobsStorage = GetEnvironmentString("AzureWebJobsStorage");
        public static readonly string tenantId = GetEnvironmentString("tenantId");
        public static readonly string clientId = GetEnvironmentString("clientId");
        public static readonly string keyVaultUrl = GetEnvironmentString("keyVaultUrl");
        public static readonly string secretNameClient = GetEnvironmentString("secretNameClient");
        public static readonly string delegateEmail = GetEnvironmentString("delegateEmail");
        public static readonly string secretNameDelegatePassword = GetEnvironmentString("secretNameDelegatePassword");
        public static readonly string siteId = GetEnvironmentString("siteId");
        public static readonly string listId = GetEnvironmentString("listId");
        public static readonly string containerName = GetEnvironmentString("containerName");
        public static readonly string deleteFunctionUrl = GetEnvironmentString("deleteFunctionUrl");

        private static string GetEnvironmentString(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine($"Warning: Environment variable '{name}' is not set.");
            }
            return value;
        }
    }
}
