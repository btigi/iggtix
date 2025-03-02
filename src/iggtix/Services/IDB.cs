namespace iggtix.Services
{
    public interface IDB
    {
        Task<bool> InitializeDatabase();
        Task<bool> AddCommand(string trigger, string response);
        Task<bool> DeleteCommand(string trigger);
        Task<List<string>> GetCommand(string trigger);
        Task<bool> AddAncillary(string trigger, string response);
        Task<bool> DeleteAncillary(string trigger, string response);
    }
}