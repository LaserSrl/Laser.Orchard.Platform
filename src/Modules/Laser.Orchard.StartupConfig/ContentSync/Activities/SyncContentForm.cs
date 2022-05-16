using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Laser.Orchard.StartupConfig.ContentSync.Activities {
    public class SyncContentForm : IFormProvider {
        protected dynamic _shapeFactory { get; set; }
        private readonly IContentManager _contentManager;
        public Localizer T { get; set; }

        public SyncContentForm(IShapeFactory shapeFactory, IContentManager contentManager) {
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {

            Func<IShapeFactory, dynamic> form =
              shape => {
                  var f = _shapeFactory.Form(
                      Id: "SyncContentForm",
                      _Type: _shapeFactory.Textbox(
                        Name: "TargetType",
                        Title: T("Target Type")
                      ),
                       _Versioning: _shapeFactory.Checkbox(
                        Name: "Versioning",
                        Value: "True",
                        Title: "Ensure new version"
                    ),
                       _Publishing: _shapeFactory.Checkbox(
                        Name: "Publishing",
                        Value: "True",
                        Title: "Ensure publish"
                    ),
                       _Creating: _shapeFactory.Checkbox(
                        Name: "Creating",
                        Value: "True",
                        Title: "Ensure creation"
                    ));
                  return f;
              };
            context.Form("SyncContentForm", form);
        }
    }
}
