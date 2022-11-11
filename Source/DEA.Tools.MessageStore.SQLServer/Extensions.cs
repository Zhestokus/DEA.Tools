using DEA.Core;

namespace DEA.Tools.MessageStore.SQLServer
{
    public static class Extensions
    {
        public static IDeaConnector UseSQLServerStore(this IDeaConnector deaConnetor, string connectionString, string tableName)
        {
            var instance = deaConnetor.UseStore(() => new SQLServerMessageStore(connectionString, tableName));
            return instance;
        }

        public static IDeaProcessor UseSQLServerStore(this IDeaProcessor deaProcessor, string connectionString, string tableName)
        {
            var instance = deaProcessor.UseStore(() => new SQLServerMessageStore(connectionString, tableName));
            return instance;
        }
    }
}
