using System;

namespace Contrib.Reviews.Models {
    public class Review {
        public int Id { get; set; }
        public string UserName { get; set; }
        public Rating Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedUtc { get; set; }
        public bool IsCurrentUsersReview { get; set; }
    }
}