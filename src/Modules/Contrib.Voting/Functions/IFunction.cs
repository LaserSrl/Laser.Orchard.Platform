using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Contrib.Voting.Models;
using Orchard;

namespace Contrib.Voting.Functions {
    /// <summary>
    /// Whenever a VoteRecord is created, updated or deleted, this method is called so that calculations can occur
    /// </summary>
    public interface IFunction : IDependency {
        string Name { get; }

        // Wether the function can calculate the result based on an updated vote. If false, all votes are submitted for calculation
        bool CanUpdate { get; }

        // Wether the function can calculate the result based on a new vote only. If false, all votes are submitted for calculation
        bool CanCreate { get; }

        // Wether the function can calculate the result based on a removed vote only. If false, all votes are submitted for calculation
        bool CanDelete { get; }

        // Calculates the result based on all votes
        void Calculate(IEnumerable<VoteRecord> votes, int contentId, out double result);

        // Calculates the result based on a new vote
        void Create(double previousResult, double previousCount, double vote, out double result);

        // Calculates the result based on an updated vote
        void Update(double previousResult, double previousCount, double previousVote, double newVote, out double result);

        // Calculates the result based on an removed vote
        void Delete(double previousResult, double previousCount, double previousVote, out double result);
    }
}