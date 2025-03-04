namespace iggtix.Services
{
    public interface IDB
    {
        Task<bool> InitializeDatabase();
        Task<bool> AddCommand(string trigger, string response, bool modOnly);
        Task<bool> DeleteCommand(string trigger);
        Task<List<(string text, bool modOnly)>> GetCommand(string trigger);
        Task<bool> AddAncillary(string trigger, string response);
        Task<bool> DeleteAncillary(string trigger, string response);
    }
}