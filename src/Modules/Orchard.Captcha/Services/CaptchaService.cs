using System.Collections.Specialized;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using Orchard.Captcha.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Settings;

namespace Orchard.Captcha.Services {
    public class CaptchaService : ICaptchaService {
        private const string ChallengeFieldKey = "recaptcha_challenge_field";
        private const string ResponseFieldKey = "recaptcha_response_field";
        private string CurrentCulture { get; set; }
        private CaptchaSettingsPart _captchaSettings;
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public CaptchaService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public string GenerateCaptcha() {
            _captchaSettings = _orchardServices.WorkContext.CurrentSite.As<CaptchaSettingsPart>();
            var CurrentCulture = _orchardServices.WorkContext.CurrentCulture;

            if (_captchaSettings == null) {
                return "CAPTCHA Part Record Was Not Found";
            }

            var captchaControl = new Recaptcha.RecaptchaControl {
                ID = "recaptcha",
                PublicKey = _captchaSettings.PublicKey,
                PrivateKey = _captchaSettings.PrivateKey,
                Theme = _captchaSettings.Theme,
                Language = CurrentCulture.Substring(0, 2)
            };

            string captchaMarkup;
            if (captchaControl.Theme.ToLower() != "custom") {
                var htmlWriter = new HtmlTextWriter(new StringWriter());
                captchaControl.RenderControl(htmlWriter);
                captchaMarkup = htmlWriter.InnerWriter.ToString();
            } else {
                if (_captchaSettings.CustomCaptchaMarkup != string.Empty) {
                    captchaMarkup = _captchaSettings.CustomCaptchaMarkup;
                } else {
                    captchaMarkup = "<script type=\"text/javascript\">var RecaptchaOptions = {theme: 'custom', custom_theme_widget: 'recaptcha_widget', lang: '" + CurrentCulture.Substring(0, 2) + "' };</script>";
                    captchaMarkup += "<div id=\"recaptcha_widget\" style=\"display:none\"><div id=\"recaptcha_image\"></div><div class=\"recaptcha_only_if_incorrect_sol\">" + T("Incorrect please try again") + "</div><span class=\"recaptcha_only_if_image\">" + T("Enter the words above") + "</span><span class=\"recaptcha_only_if_audio\">" + T("Enter the numbers you hear:") + "</span><input type=\"text\" id=\"recaptcha_response_field\" name=\"recaptcha_response_field\" /><div><a href=\"javascript:Recaptcha.reload()\">" + T("Get another CAPTCHA") + "</a></div><div class=\"recaptcha_only_if_image\"><a href=\"javascript:Recaptcha.switch_type('audio')\">" + @T("Get an audio CAPTCHA") + "</a></div><div class=\"recaptcha_only_if_audio\"><a href=\"javascript:Recaptcha.switch_type('image')\">" + @T("Get an image CAPTCHA") + "</a></div><div><a href=\"javascript:Recaptcha.showhelp()\">Help</a></div></div><script type=\"text/javascript\" src=\"http://www.google.com/recaptcha/api/challenge?k=" + captchaControl.PublicKey + "\"></script><noscript><iframe src=\"http://www.google.com/recaptcha/api/noscript?k=" + captchaControl.PublicKey + "\" height=\"300\" width=\"500\" frameborder=\"0\"></iframe><br><textarea name=\"recaptcha_challenge_field\" rows=\"3\" cols=\"40\"></textarea><input type=\"hidden\" name=\"recaptcha_response_field\" value=\"manual_challenge\"></noscript>";
                }
            }

            return captchaMarkup;
        }

        public bool IsCaptchaValid(FormCollection form, string userHostAddress) {
            var captchaChallengeValue = form[ChallengeFieldKey];
            var captchaResponseValue = form[ResponseFieldKey];
            _captchaSettings = _orchardServices.WorkContext.CurrentSite.As<CaptchaSettingsPart>();

            var captchaValidtor = new Recaptcha.RecaptchaValidator {
                PrivateKey = _captchaSettings.PrivateKey,
                RemoteIP = userHostAddress,
                Challenge = captchaChallengeValue,
                Response = captchaResponseValue
            };

            var recaptchaResponse = captchaValidtor.Validate();

            // this will push the result value into a parameter in our Action   
            return recaptchaResponse.IsValid;
        }

        public bool IsCaptchaValid(NameValueCollection form, string userHostAddress) {
            var captchaChallengeValue = form[ChallengeFieldKey];
            var captchaResponseValue = form[ResponseFieldKey];
            _captchaSettings = _orchardServices.WorkContext.CurrentSite.As<CaptchaSettingsPart>();

            var captchaValidtor = new Recaptcha.RecaptchaValidator {
                PrivateKey = _captchaSettings.PrivateKey,
                RemoteIP = userHostAddress,
                Challenge = captchaChallengeValue,
                Response = captchaResponseValue
            };

            var recaptchaResponse = captchaValidtor.Validate();

            // this will push the result value into a parameter in our Action   
            return recaptchaResponse.IsValid;
        }

        public bool IsCaptchaValid(string captchaChallengeValue, string captchaResponseValue, string userHostAddress) {
            _captchaSettings = _orchardServices.WorkContext.CurrentSite.As<CaptchaSettingsPart>();
            var captchaValidtor = new Recaptcha.RecaptchaValidator {
                PrivateKey = _captchaSettings.PrivateKey,
                RemoteIP = userHostAddress,
                Challenge = captchaChallengeValue,
                Response = captchaResponseValue
            };

            var recaptchaResponse = captchaValidtor.Validate();

            // this will push the result value into a parameter in our Action   
            return recaptchaResponse.IsValid;
        }
    }
}
