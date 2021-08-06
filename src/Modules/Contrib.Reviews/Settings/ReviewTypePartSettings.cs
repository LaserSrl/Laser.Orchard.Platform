namespace Contrib.Reviews.Settings {
    public class ReviewTypePartSettings {
        private bool? _showStars;
        private bool? _showReviews;
        public bool ShowStars {
            get {
                if (_showStars == null)
                    _showStars = true;
                return (bool)_showStars;
            }
            set { _showStars = value; }
        }
        public bool ShowReviews {
            get {
                if (_showReviews == null)
                    _showReviews = true;
                return (bool)_showReviews;
            }
            set { _showReviews = value; }
        }
    }
}