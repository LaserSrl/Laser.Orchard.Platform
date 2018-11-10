using System.Collections.Specialized;
using System.Web.Mvc;
using Orchard.Captcha.Models;

namespace Orchard.Captcha.Services {
    public interface ICaptchaService : IDependency {
        string GenerateCaptcha();
        bool IsCaptchaValid(FormCollection form, string userHostAddress);
        bool IsCaptchaValid(NameValueCollection form, string userHostAddress);
        bool IsCaptchaValid(string captchaChallengeValue, string captchaResponseValue, string userHostAddress);
    }
}
