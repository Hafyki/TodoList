namespace TodoList.Services
{
    public interface ISyncService
    {
        Task SyncData(string url);
    }
}
