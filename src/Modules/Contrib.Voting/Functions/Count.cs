using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Voting.Models;

namespace Contrib.Voting.Functions {
    public class Count : IFunction {
        public string Name {
            get { return "count"; }
        }

        public bool CanUpdate {
            get { return true; }
        }

        public bool CanCreate {
            get { return true; }
        }

        public bool CanDelete {
            get { return true; }
        }

        public void Calculate(IEnumerable<VoteRecord> votes, int contentId, out double result) {
            result = votes.Count();
        }

        public void Create(double previousResult, double previousCount, double vote, out double result) {
            result = previousResult + 1;
        }

        public void Update(double previousResult, double previousCount, double previousVote, double newVote, out double result) {
            result = previousResult;
        }

        public void Delete(double previousResult, double previousCount, double previousVote, out double result) {
            result = previousResult - 1;
        }
    }
}