using Laser.Orchard.GDPR.Models;
using Laser.Orchard.GDPR.Permissions;
using Laser.Orchard.GDPR.Services;
using Laser.Orchard.GDPR.Settings;
using Laser.Orchard.GDPR.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.GDPR.Drivers {
    public class GDPRPartDriver : ContentPartDriver<GDPRPart> {

        private readonly IAuthorizer _authorizer;
        private readonly IEnumerable<IGDPRProcessAllowedProvider> _GDPRProcessAllowedProviders;

        public GDPRPartDriver(
            IAuthorizer authorizer,
            IEnumerable<IGDPRProcessAllowedProvider> GDPRProcessAllowedProviders) {

            _authorizer = authorizer;
            _GDPRProcessAllowedProviders = GDPRProcessAllowedProviders;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "GDPRPart"; }
        }

        protected override DriverResult Display(GDPRPart part, string displayType, dynamic shapeHelper) {
            var shapes = new List<DriverResult>();
            // Profile Items have the links to process them
            // Here we compute a shape with "Anonymize" and "Erase" links.
            // Either link, can only be displayed if this is a ProfileItem, and the current user
            // has the corresponding permissions, and the item is not protected.
            if (part.TypePartDefinition.Settings.GetModel<GDPRPartTypeSettings>()?.IsProfileItemType == true
                && (_authorizer.Authorize(GDPRPermissions.ManageAnonymization, part)
                    || _authorizer.Authorize(GDPRPermissions.ManageErasure, part))) {
                // user can manage this Profile Item

                if (_GDPRProcessAllowedProviders.Any(gpa => !gpa.ProcessIsAllowed(part))) {
                    // Item is protected and cannot be processed for GDPR. This may not be because the
                    // IsProtected flag is set in the part.
                    shapes.Add(ContentShape("Parts_GDPRPart_AdminToolbarLink",
                        () => shapeHelper.Parts_GDPRPart_AdminToolbarLink(
                            Model: new GDPRPartAdminToolbarLinkViewModel {
                                LinkText = T("Cannot perform GDPR Processes").Text,
                                LinkAction = string.Empty, // this will not render as an action link
                                ItemId = part.Id
                            })));
                } else {
                    if (_authorizer.Authorize(GDPRPermissions.ManageAnonymization, part)) {
                        // Add the shape with the "Anonymize" link
                        shapes.Add(ContentShape("Parts_GDPRPart_AdminToolbarLink",
                            () => shapeHelper.Parts_GDPRPart_AdminToolbarLink(
                                Model: new GDPRPartAdminToolbarLinkViewModel {
                                    LinkText = T("Anonymize").Text,
                                    LinkAction = "Anonymize",
                                    ItemId = part.Id
                                })));
                    }
                    if (_authorizer.Authorize(GDPRPermissions.ManageErasure, part)) {
                        // Add the shape with the "Erase" link
                        shapes.Add(ContentShape("Parts_GDPRPart_AdminToolbarLink",
                            () => shapeHelper.Parts_GDPRPart_AdminToolbarLink(
                                Model: new GDPRPartAdminToolbarLinkViewModel {
                                    LinkText = T("Erase").Text,
                                    LinkAction = "Erase",
                                    ItemId = part.Id
                                })));
                    }
                }
            }

            if (_authorizer.Authorize(GDPRPermissions.ManageItemProtection, part)) {
                // add a shape that give a link to control the IsProtected flag on the item, because some
                // items may not be editable, but we still want to control that flag.
                if (part.IsProtected) {
                    shapes.Add(ContentShape("Parts_GDPRPart_AdminToolbarLink",
                        () => shapeHelper.Parts_GDPRPart_AdminToolbarLink(
                            Model: new GDPRPartAdminToolbarLinkViewModel {
                                LinkText = T("Reset Protection").Text,
                                LinkAction = "ResetProtection",
                                ItemId = part.Id
                            })));
                } else {
                    shapes.Add(ContentShape("Parts_GDPRPart_AdminToolbarLink",
                        () => shapeHelper.Parts_GDPRPart_AdminToolbarLink(
                            Model: new GDPRPartAdminToolbarLinkViewModel {
                                LinkText = T("Set protection").Text,
                                LinkAction = "SetProtection",
                                ItemId = part.Id
                            })));
                }
            }
            
            return Combined(shapes.ToArray());
        }

        protected override DriverResult Editor(GDPRPart part, dynamic shapeHelper) {
            var shapes = new List<DriverResult>();

            if (_authorizer.Authorize(GDPRPermissions.ManageItemProtection, part)) {
                var vm = MakeViewModel(part);
                shapes.Add(ContentShape("Parts_GDPRPart_ProtectionFlag",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/GDPRPartProtectionFlag",
                        Model: vm,
                        Prefix: Prefix
                        )));
            }


            return Combined(shapes.ToArray());
        }

        protected override DriverResult Editor(GDPRPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (_authorizer.Authorize(GDPRPermissions.ManageItemProtection, part)) {
                var vm = new GDPRPartEditorViewModel();
                if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                    part.IsProtected = vm.IsProtected;
                }
            }

            return Editor(part, shapeHelper);
        }

        /// <summary>
        /// Create a ViewModel based on the part provided. This method is really simple to begin
        /// with, to the point where it's not even really needed, but I create it already so that when
        /// the model becomes more complex I don't have to refactor code.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private GDPRPartEditorViewModel MakeViewModel(GDPRPart part) {
            return new GDPRPartEditorViewModel {
                IsProtected = part.IsProtected
            };
        }
    }
}