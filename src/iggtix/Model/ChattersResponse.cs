public class ChattersResponse
{
    public UserData[] data { get; set; }
    public Pagination pagination { get; set; }
    public int total { get; set; }
}

public class UserData
{
    public string user_id { get; set; }
    public string user_login { get; set; }
    public string user_name { get; set; }
}

public class Pagination
{
}