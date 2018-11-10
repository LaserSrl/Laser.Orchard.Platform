using Laser.Orchard.Mobile.Models;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Projections {
    public interface IFormProvider : IEventHandler { void Describe(DescribeContext context); }

    public class DeviceTypeForm : IFormProvider {

        public Localizer T { get; set; }
        protected dynamic Shape { get; set; }

        public DeviceTypeForm(IShapeFactory shapeFactory) {
			Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "DeviceTypeForm",
                        _DeviceTypes: Shape.SelectList(
                            Id: "DeviceType", Name: "DeviceType",
                            Title: T("Device Type"),
                            Description: T("The type of the device the contact registered with (i.e. Android, Apple, Windows)."),
                            Size: Enum.GetValues(typeof(TipoDispositivo)).Length,
                            Multiple: false
                            )
                        );

                    foreach (TipoDispositivo value in Enum.GetValues(typeof(TipoDispositivo))) {
                        f._DeviceTypes.Add(new SelectListItem { Value = value.ToString(), Text = value.ToString() });
                    }

                    return f;
                };

                context.Form("DeviceTypeForm", form);
        }
    }
}