using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.ViewModels;
using Orchard;

namespace Laser.Orchard.NwazetIntegration.Services {
    public interface IGTMProductService : IDependency {
        void FillPart(GTMProductPart part);
        /// <summary>
        /// Fills the part based on settings and returns it as a string
        /// for its JSON representation.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        /// <remarks>This method is designed for use in shapes where we need to be able to
        /// provide the data for the dataLayer, but we have not gone through the driver
        /// to get the shapes in place.</remarks>
        string GetJsonString(GTMProductPart part);
        string GetJsonString(GTMProductVM vm);
        string GetJsonString(GTMActionField af);
    }

}