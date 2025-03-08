using iggtix.Interface.Models;

namespace iggtix.Interface
{
    public interface ITwitchApi
    {
        Task<ChannelInfo> GetChannelInfo();
        Task<T> GetChatters<T>();
        Task<T> GetUserInfo<T>(string loginName);
    }
}