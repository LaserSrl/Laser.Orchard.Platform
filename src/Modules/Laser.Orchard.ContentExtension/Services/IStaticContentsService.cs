using Orchard;

namespace Laser.Orchard.ContentExtension.Services
{
    public interface IStaticContentsService : ISingletonDependency
    {
        string GetBaseFolder();

        bool StaticContentIsAllowed(string filePath);
    }
}
