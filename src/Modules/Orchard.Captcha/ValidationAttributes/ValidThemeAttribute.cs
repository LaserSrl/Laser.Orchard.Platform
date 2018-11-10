using System;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Captcha.ValidationAttributes
{
    public class ValidThemeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var theme = Themes.red;
            var stringToValidate = ((string)value).ToLower();
            return Enum.TryParse<Themes>(stringToValidate, out theme);
        }
    }
}