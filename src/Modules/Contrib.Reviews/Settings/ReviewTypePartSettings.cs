namespace Contrib.Reviews.Settings {
    public class ReviewTypePartSettings {
        private bool? _showStars;
        public bool ShowStars {
            get {
                if (_showStars == null)
                    _showStars = true;
                return (bool)_showStars;
            }
            set { _showStars = value; }
        }
    }
}