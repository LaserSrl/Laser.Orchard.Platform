using Laser.Orchard.NwazetIntegration.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.NwazetIntegration.ApplicabilityCriteria {
    [OrchardFeature("Laser.Orchard.PickupPoints")]
    public class PickupPointsForm : IFormProvider {

        private readonly IContentManager _contentManager;
        protected dynamic Shape { get; set; }

        public static string FormName = "PickupPointsForm";

        public PickupPointsForm(
            IShapeFactory shapeFactory,
            IContentManager contentManager) {

            Shape = shapeFactory;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form = shape => {
                var f = Shape.Form(
                    Id: "anypickuppoint",
                    _PickupPoints: Shape.SelectList(
                        Id: "pickuppointids",
                        Name: "PickupPointIds",
                        Title: T("Pickup Points"),
                        Description: T("Select the pickup points."),
                        Size: 10,
                        Multiple: true
                        )
                    );

                f._PickupPoints.Add(
                    new SelectListItem { Value = string.Empty, Text = T("Any").Text });
                // Get all configured pickup points
                var pPPoints = _contentManager
                    .Query<PickupPointPart>(
                        VersionOptions.Published, 
                        // TODO: extend this to other content types
                        PickupPointPart.DefaultContentTypeName)
                    .List();
                // Add an element for each
                foreach (var part in pPPoints) {
                    var selectValue = part.Id.ToString();
                    var selectText = string.Join(". ",
                        (new string[] {
                            _contentManager.GetItemMetadata(part).DisplayText.Trim(),
                            part.AddressText().Trim()
                        }).Where(s => !string.IsNullOrWhiteSpace(s)));
                    f._PickupPoints.Add(
                        new SelectListItem { Value = selectValue, Text = selectText });
                }

                return f;
            };
            
            context.Form(FormName, 
                form,
                Import,
                Export
                );
        }

        public void Export(dynamic state, ExportContentContext context) {
            string pickupPointIds = Convert.ToString(state.PickupPointIds);
            if (!string.IsNullOrWhiteSpace(pickupPointIds)) {
                // From the ids to the identities
                // There may be the empty string (that's the value for "Any" pickup point).
                // In that case, might as well only export that.
                var idStrings = pickupPointIds.Split(',');
                if (idStrings.Any(s => s.Length == 0)) {
                    state.PickupPointIds = "";
                } else {
                    // Prevent exceptions here from breaking the export process
                    var ids = idStrings
                        .Select(s => {
                            int i = 0;
                            if (!int.TryParse(s, out i)) {
                                i = -1;
                            }
                            return i;
                        }).Where(i => i > 0);
                    var contents = _contentManager
                        .GetMany<PickupPointPart>(ids, VersionOptions.Published, QueryHints.Empty);
                    var identities = contents
                        .Select(context.ContentManager.GetItemMetadata)
                        .Select(x => x.Identity.ToString()).ToArray();

                    state.PickupPointIds = string.Join(",", identities);
                }
            }
        }

        public void Import(dynamic state, ImportContentContext context) {
            string pointIdentities = Convert.ToString(state.PickupPointIds);
            if (!string.IsNullOrEmpty(pointIdentities)) {
                // From identities to ids
                // There may be the empty string (that's the value for "Any" pickup point).
                // In that case, might as well only import that.
                var identities = pointIdentities.Split(new[] { ',' });
                if (identities.Any(s => s.Length == 0)) {
                    state.PickupPointIds = "";
                } else {
                    var contents = identities
                        .Select(context.GetItemFromSession);
                    var ids = contents
                        .Select(x => x.Id)
                        .Select(x => x.ToString()).ToArray();

                    state.PickupPointIds = string.Join(",", ids);
                }
            }
        }
    }
}