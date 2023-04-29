﻿using RpgCollector.Models;

namespace RpgCollector.RequestResponseModel.LoginModel
{
    public class LoginResponse
    {
        public ErrorState Error { get; set; }
        public string UserName { get; set; }
        public string AuthToken { get; set; }
    }
}