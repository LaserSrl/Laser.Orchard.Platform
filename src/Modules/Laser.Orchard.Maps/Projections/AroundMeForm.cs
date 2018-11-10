using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Localization;

namespace Laser.Orchard.Maps.Projections {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

    public class AroundMeForm : IFormProvider {
        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        public AroundMeForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }
        public void Describe(dynamic context) {
            Func<IShapeFactory, dynamic> form =
                shape => {

                    var f = Shape.Form(
                        Id: "AroundMeForm",
                        _Center: Shape.TextBox(
                            Id: "Center", Name: "Center",
                            Title: T("Position coords"),
                            Description: T("Comma separated Latitude,Longitude central position."),
                            Classes: new[] { "text medium", "tokenized" }
                            ),
                        _Distance: Shape.TextBox(
                            Id: "Distance", Name: "Distance",
                            Title: T("Distance"),
                            Description: T("Distance from position in meters."),
                            Classes: new[] { "text medium", "tokenized" }
                            ));
                    return f;
                };

            context.Form("AroundMeForm", form);

        }

    }
}

