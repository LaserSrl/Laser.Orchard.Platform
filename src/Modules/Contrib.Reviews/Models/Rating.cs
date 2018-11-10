using Contrib.Voting.Models;

namespace Contrib.Reviews.Models {
    public class Rating {
        public ResultRecord CurrentVotingResult { get; set; }
        public double UserRating { get; set; }
    }
}