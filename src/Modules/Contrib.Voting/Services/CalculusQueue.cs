using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;

namespace Contrib.Voting.Services
{
    public interface ICalculusQueue : IEnumerable<Calculus>, ISingletonDependency {
        void Enqueue(Calculus c);
        Calculus Dequeue();
        int Count { get; }
    }

    public class CalculusQueue : Queue<Calculus>, ICalculusQueue  {
    }
}