using Orchard;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Settings;
using Orchard.Security;
using System;
using Laser.Orchard.StartupConfig.Services;

namespace Contrib.Profile.Services {
    public interface IFrontEndProfileService : IFrontEndEditService {

        /// <summary>
        /// Checks that the IUser does not have a ProfilePart in its implementation.
        /// </summary>
        /// <param name="user">A user object.</param>
        /// <returns>true, if no ProfilePart can be found on the user object.</returns>
        bool UserHasNoProfilePart(IUser user);

        /// <summary>
        /// A delegate for the tests to decide which parts may be displayed on the front end.
        /// </summary>
        Func<ContentTypePartDefinition, string, bool> MayAllowPartDisplay { get; }
        /// <summary>
        /// A delegate for the tests to decide which parts may be edited on the front end.
        /// </summary>
        Func<ContentTypePartDefinition, string, bool> MayAllowPartEdit { get; }
        /// <summary>
        /// A delegate for the tests to decide which fields may be displayed on the front end.
        /// </summary>
        Func<ContentPartFieldDefinition, bool> MayAllowFieldDisplay { get; }
        /// <summary>
        /// A delegate for the tests to decide which fields may be edited on the front end.
        /// </summary>
        Func<ContentPartFieldDefinition, bool> MayAllowFieldEdit { get; }

        /// <summary>
        /// Get the FrontEnd Editor Placement information for the type defined by the argument.
        /// </summary>
        /// <param name="contentTypeDefinition">The definition of the type.</param>
        /// <returns>An array of the FrontEnd editor PlacementSettings for the parts and fields in the type.</returns>
        PlacementSettings[] GetFrontEndPlacement(ContentTypeDefinition contentTypeDefinition);

    }
}
