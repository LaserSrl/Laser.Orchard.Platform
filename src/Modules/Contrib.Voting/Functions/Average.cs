using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Voting.Models;

namespace Contrib.Voting.Functions {
    public class Average : IFunction {
        public string Name {
            get { return "average"; }
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
            result = votes.Average(vote => vote.Value);
        }

        public void Create(double previousResult, double previousCount, double vote, out double result) {
            result = (previousResult*previousCount + vote)/(previousCount + 1);
        }

        public void Update(double previousResult, double previousCount, double previousVote, double newVote, out double result) {
            if (previousCount != 0) {
                result = (previousResult*previousCount - previousVote + newVote)/(previousCount);
            }
            else {
                result = 0;
            }
        }

        public void Delete(double previousResult, double previousCount, double previousVote, out double result) {
            if (previousCount - 1 != 0) {
                result = (previousResult*previousCount - previousVote)/(previousCount - 1);
            }
            else {
                result = 0;
            }
        }
    }
}