﻿public class RefreshTokenResponse
{
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public string[] scope { get; set; }
    public string token_type { get; set; }

    public string error { get; set; }
    public string status { get; set; }
    public string message { get; set; }
}