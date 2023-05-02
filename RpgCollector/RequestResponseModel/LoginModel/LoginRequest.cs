﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using RpgCollector.CustomConstraints;
using System.ComponentModel.DataAnnotations;

namespace RpgCollector.RequestResponseModel.LoginModel
{
    public class LoginRequest
    {
        [Required]
        [LoginUserName]
        public string UserName { get; set; }

        [Required]
        [LoginUserPassword]
        public string Password { get; set; }
    }
}
