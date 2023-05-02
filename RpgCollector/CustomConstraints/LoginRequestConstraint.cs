using Humanizer.Localisation;
using RpgCollector.RequestResponseModel.LoginModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


/*
    Register과 Login시 날라오는 UserId, Password에 대해서 정규표현식 사용
 */
namespace RpgCollector.CustomConstraints
{
    public class LoginUserNameAttribute : ValidationAttribute
    {
        public string GetErrorMessage()
        {
            return "Invalid String";
        }

        protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
        { 
            Regex regex = new Regex("^(?=^\\w{3,20}$)[a-z0-9]+_?[a-z0-9]+$");
            LoginRequest userRequest = (LoginRequest)validationContext.ObjectInstance;
            if (!regex.IsMatch(userRequest.UserName))
            {
                return new ValidationResult(GetErrorMessage());
            }
            return ValidationResult.Success;
        }
    }

    public class LoginUserPasswordAttribute : ValidationAttribute
    {
        public string GetErrorMessage()
        {
            return "Invalid String";
        }

        protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
        {
            Regex regex = new Regex("^(?=^\\w{3,20}$)[a-z0-9]+_?[a-z0-9]+$");
            LoginRequest userRequest = (LoginRequest)validationContext.ObjectInstance;
            if (!regex.IsMatch(userRequest.Password))
            {
                return new ValidationResult(GetErrorMessage());
            }
            return ValidationResult.Success;
        }
    }
}
