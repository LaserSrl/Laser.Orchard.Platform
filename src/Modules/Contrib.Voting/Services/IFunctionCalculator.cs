using Orchard;

namespace Contrib.Voting.Services {
    public interface IFunctionCalculator : IDependency {
        void Calculate(Calculus calculus);
    }
}
