using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.MediaLibrary.Models;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using Orchard.Security;
using Orchard.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Orchard.Utility.Extensions;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.StartupConfig.Controllers {

    [OrchardFeature("Laser.Orchard.StartupConfig.MediaExtensions")]
    public class MediaTransformController : Controller {
       // private readonly Work<IImageProfileManager> _imageProfileManager;
        private readonly IImageProfileManager _imageProfileManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //  public MediaTransformController(Work<IImageProfileManager> imageProfileManager, IOrchardServices orchardServices, IHttpContextAccessor httpContextAccessor) {
        public MediaTransformController(IImageProfileManager imageProfileManager, IOrchardServices orchardServices, IHttpContextAccessor httpContextAccessor) {
            _imageProfileManager = imageProfileManager;
            _orchardServices = orchardServices;
            _httpContextAccessor = httpContextAccessor;
        }

        [AlwaysAccessible]
        public RedirectResult Image(string Path, int? Width, int? Height, string Mode, string Alignment, string PadColor) {
            var httpContext = _httpContextAccessor.Current();
            int n = 0;
            string url;
            bool isNumeric = int.TryParse(Path, out n);
            if (isNumeric) {
                MediaPart mediaPart = ((ContentItem)_orchardServices.ContentManager.Get(n)).As<MediaPart>();
                Path = mediaPart.MediaUrl;
            }

            if (Width == null || Height == null) {
                var baseUrl = httpContext.Request.ToApplicationRootUrlString();
                var applicationPath = httpContext.Request.ApplicationPath ?? String.Empty;
                if (Path.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase)) {
                    Path = Path.Substring(applicationPath.Length);
                }
                url = String.Concat(baseUrl, Path);
            } else {
                int notNullWidth = Width.GetValueOrDefault();
                int notNullHeight = Height.GetValueOrDefault();

                var state = new Dictionary<string, string> {
                {"Width", notNullWidth.ToString(CultureInfo.InvariantCulture)},
                {"Height", notNullHeight.ToString(CultureInfo.InvariantCulture)},
                {"Mode", Mode},
                {"Alignment", Alignment},
                {"PadColor", PadColor},
            };

                var filter = new FilterRecord {
                    Category = "Transform",
                    Type = "Resize",
                    State = FormParametersHelper.ToString(state)
                };

                var profile = "Transform_Resize"
                    + "_w_" + Convert.ToString(Width)
                    + "_h_" + Convert.ToString(Height)
                    + "_m_" + Convert.ToString(Mode)
                    + "_a_" + Convert.ToString(Alignment)
                    + "_c_" + Convert.ToString(PadColor);
                Path = HttpUtility.UrlDecode(Path);
                //  url = _imageProfileManager.Value.GetImageProfileUrl(Path, profile, filter);
                url = _imageProfileManager.GetImageProfileUrl(Path, profile, filter);
            }
            if (url == null) {
                return null;
            }
            return Redirect(url);
        }
    }
}