using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Contrib.Reviews.Models {
    public class ReviewsPart : ContentPart {

        public ReviewsPart() {
            Rating = new Rating();
            Reviews = new List<Review>();
        }

        public bool ShowStars { get; set; }
        public Rating Rating { get; set; }
        public List<Review> Reviews { get; set; }
        public bool UserHasReviewed { get; set; }
    }
}