using Orchard;

namespace Laser.Orchard.Reporting.Providers {
    public interface IGroupByParameterProvider : IDependency
    {
        void Describe(DescribeGroupByContext context);
    }
}