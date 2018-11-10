using System.ComponentModel.DataAnnotations;
using Orchard.Captcha.ValidationAttributes;
using Orchard.ContentManagement;

namespace Orchard.Captcha.Models
{
    public class CaptchaSettingsPart : ContentPart<CaptchaSettingsPartRecord>
    {
        [Required]
        public string PublicKey
        {
            //get { return Record.PublicKey; }
            //set { Record.PublicKey = value; }
            get { return Retrieve(r => r.PublicKey); }
            set { Store(r => r.PublicKey, value); }
        }

        [Required]
        public string PrivateKey
        {
            //get { return Record.PrivateKey; }
            //set { Record.PrivateKey = value; }
            get { return Retrieve(r => r.PrivateKey); }
            set { Store(r => r.PrivateKey, value); }
        }

        [Required]
        [ValidTheme]
        public string Theme
        {
            //get { return Record.Theme; }
            //set { Record.Theme = value; }
            get { return Retrieve(r => r.Theme); }
            set { Store(r => r.Theme, value); }
        }

        public string CustomCaptchaMarkup
        {
            //get { return Record.CustomCaptchaMarkup; }
            //set { Record.CustomCaptchaMarkup = value; }
            get { return Retrieve(r => r.CustomCaptchaMarkup); }
            set { Store(r => r.CustomCaptchaMarkup, value); }
        }
    }
}