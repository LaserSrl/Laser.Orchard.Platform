using Orchard;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Settings;
using Orchard.Security;
using System;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IFrontEndEditService : IDependency {
        /// <summary>
        /// Processes the shape to be used in front end.
        /// </summary>
        /// <param name="shape">The shape to be processed. 
        /// This should be the result of one of the following IContentManager methods:
        /// BuildDisplay, BuildEditor or UpdateEditor.</param>
        /// <param name="partTest">A delegate for the tests to decide which parts may be displayed.</param>
        /// <param name="fieldTest">A delegate for the tests to decide which fileds may be displayed.</param>
        /// <returns>A dynamic object for a shape to be used for front end.</returns>
        /// <remarks>This method is used for both Display and Edit on the front-end. Developers must 
        /// ensure that the shape and the delegates match each others context correctly.</remarks>
        dynamic BuildFrontEndShape(
            dynamic shape,
            Func<ContentTypePartDefinition, string, bool> partTest,
            Func<ContentPartFieldDefinition, bool> fieldTest);

    }
}
