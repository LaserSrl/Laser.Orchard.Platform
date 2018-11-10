using Orchard.ContentManagement.Records;

namespace Orchard.Captcha.Models
{
    public class CaptchaSettingsPartRecord : ContentPartRecord
    {
        public virtual string PublicKey { get; set; }
        public virtual string PrivateKey { get; set; }
        public virtual string Theme { get; set; }
        public virtual string CustomCaptchaMarkup { get; set; }
    }

    
}