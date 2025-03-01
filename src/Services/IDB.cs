namespace iggtix.Services
{
    public interface IDB
    {
        Task<bool> InitializeDatabase();
        Task<bool> AddCommand(string trigger, string response);
        Task<bool> DeleteCommand(string trigger);
        Task<string> GetCommand(string trigger);
    }
}