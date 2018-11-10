using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Laser.Orchard.CommunicationGateway {

    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageCampaigns = new Permission { Description = "Manage Comunication Campaigns", Name = "ManageCampaigns"};
        public static readonly Permission ManageCommunicationAdv = new Permission { Description = "Manage Comunication Messages", Name = "ManageCommunicationAdv", ImpliedBy = new[] {ManageCampaigns} };
        public static readonly Permission ManageContact = new Permission { Description = "Manage Comunication Contact", Name = "ManageContact", ImpliedBy = new[] { ManageCampaigns } };
        public static readonly Permission ShowMenuCommunication = new Permission { Description = "Show Menu Communication", Name = "ShowMenuCommunication", ImpliedBy = new[] { ManageCampaigns, ManageCommunicationAdv, ManageContact } };
        public static readonly Permission ManageOwnCampaigns = new Permission { Description = "Manage Own Comunication Campaigns", Name = "ManageOwnCampaigns" , ImpliedBy = new[] { ManageCampaigns } };
        public static readonly Permission ManageOwnCommunicationAdv = new Permission { Description = "Manage Own Comunication Messages", Name = "ManageOwnCommunicationAdv", ImpliedBy = new[] { ManageCampaigns, ManageCommunicationAdv } };
        public static readonly Permission ManageOwnContact = new Permission { Description = "Manage Own Comunication Contact", Name = "ManageOwnContact", ImpliedBy = new[] { ManageCampaigns, ManageContact } };
        public static readonly Permission PublishCommunicationAdv = new Permission { Description = "Publish Comunication Messages", Name = "PublishCommunicationAdv", ImpliedBy = new[] { ManageCampaigns, ManageCommunicationAdv } };
        public static readonly Permission PublishOwnCommunicationAdv = new Permission { Description = "Publish Own Comunication Messages", Name = "PublishOwnCommunicationAdv", ImpliedBy = new[] { PublishCommunicationAdv,ManageCampaigns, ManageCommunicationAdv } };
        public static readonly Permission AccessExportContacts = new Permission { Description = "Access contacts export files", Name = "AccessExportContacts" };
        public static readonly Permission AccessImportContacts = new Permission { Description = "Access contacts import logs", Name = "AccessImportContacts" };
        public static readonly Permission ShowContacts = new Permission { Description = "Show Communication Contacts", Name = "ShowCommunicationContacts", ImpliedBy = new[] { ManageContact, AccessExportContacts, AccessImportContacts } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageCampaigns,
                ManageCommunicationAdv,
                ManageContact,
                ShowMenuCommunication,
                ManageOwnCampaigns,
                ManageOwnCommunicationAdv,
                ManageOwnContact,
                PublishOwnCommunicationAdv,
                PublishCommunicationAdv,
                AccessExportContacts,
                AccessImportContacts,
                ShowContacts
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                 Permissions = new[] {ManageCampaigns,ManageCommunicationAdv,ManageContact,ShowMenuCommunication,PublishCommunicationAdv}
                },
                new PermissionStereotype {
                    Name = "Editor",
                },
                new PermissionStereotype {
                    Name = "Moderator",
                  },
                new PermissionStereotype {
                    Name = "Author",
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }
    }
}