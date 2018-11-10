using Orchard;

namespace Laser.Orchard.Accessibility.Services
{
    public interface IAccessibilityServices : IDependency
    {
        void SetTextOnly();
        void SetNormal();
        void SetHighContrast();
    }
}