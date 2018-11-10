namespace Laser.Orchard.Pdf.Models {
    public class PdfButtonSettings {
        private float _headerHeight = 10f; // default value
        private float _footerHeight = 10f; // default value
        private float _leftMargin = 50f; // default value
        private float _rightMargin = 50f; // default value
        private float _pageWidth = 595f; // default value (A4)
        private float _pageHeight = 842f; // default value (A4)
        private string _label = "PDF"; // default value
        public string Label {
            get {
                return _label;
            }
            set {
                _label = string.IsNullOrWhiteSpace(value) ? _label : value;
            }
        }
        public int Position { get; set; }
        public bool Delete { get; set; }
        public int TemplateId { get; set; }
        public bool PublishedVersionOfContent { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public float HeaderHeight {
            get {
                return _headerHeight;
            }
            set {
                _headerHeight = (value > 0) ? value : _headerHeight;
            }
        }
        public float FooterHeight {
            get {
                return _footerHeight;
            }
            set {
                _footerHeight = (value > 0) ? value : _footerHeight;
            }
        }
        public float LeftMargin {
            get {
                return _leftMargin;
            }
            set {
                _leftMargin = (value > 0) ? value : _leftMargin;
            }
        }
        public float RightMargin {
            get {
                return _rightMargin;
            }
            set {
                _rightMargin = (value > 0) ? value : _rightMargin;
            }
        }
        public float PageWidth {
            get {
                return _pageWidth;
            }
            set {
                _pageWidth = (value > 0) ? value : _pageWidth;
            }
        }
        public float PageHeight {
            get {
                return _pageHeight;
            }
            set {
                _pageHeight = (value > 0) ? value : _pageHeight;
            }
        }
    }
}