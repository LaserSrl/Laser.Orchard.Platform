using System;
using Laser.Orchard.StartupConfig.MediaExtensions.UserFolder.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.MediaLibrary.Providers;
using Orchard.Security;

namespace Laser.Orchard.StartupConfig.MediaExtensions.UserFolder.Providers {
    [OrchardFeature("Laser.Orchard.StartupConfig.MediaExtensions")]
    public class OwnedFolder : IMediaFolderProvider {
        public virtual string GetFolderName(IUser content) {
            string folder = "";
            string dirtyFolderName;
            if (content.As<OwnedFolderPart>() == null || string.IsNullOrWhiteSpace(content.As<OwnedFolderPart>().FolderName)) {
                if (content.As<IdentityPart>() != null && !string.IsNullOrWhiteSpace(content.As<IdentityPart>().Identifier)) {
                    dirtyFolderName = content.As<IdentityPart>().Identifier;
                }
                else {
                    dirtyFolderName = content.UserName;
                }
            }
            else {
                dirtyFolderName = content.As<OwnedFolderPart>().FolderName;
            }
            foreach (char c in dirtyFolderName) {
                if (char.IsLetterOrDigit(c)) {
                    folder += c;
                }
                else
                    folder += "_" + String.Format("{0:X}", Convert.ToInt32(c));
            }
            return folder;
        }
    }
}