using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Maps.Models {
    public class CustomPinPartSettings {
        private TargetArea _targetArea;

        public string ReplacedColor { get; set; }
        public string AlternateColor { get; set; }
        public string PinUrl { get; set; }
        public string WaterMarkUrl { get; set; }
        /// <summary>
        /// A serialized Json object of <see cref="TargetArea"/> objects
        /// </summary>
        public string TargetAreaDefinitions { get; set; }

        public TargetArea TargetArea {
            get {
                if (_targetArea == null) {
                    if (!string.IsNullOrWhiteSpace(TargetAreaDefinitions)) {
                        _targetArea = new DefaultJsonConverter().Deserialize<TargetArea>(TargetAreaDefinitions);
                    }
                }

                return _targetArea;
            }

            set {
                _targetArea = value;
                TargetAreaDefinitions = new DefaultJsonConverter().Serialize(_targetArea);
            }
        }
    }

    public class TargetArea {
        public string CenterX { get; set; }
        public string CenterY { get; set; }
        public string DrawableWidth { get; set; }
        public string DrawableHeight { get; set; }
    }
}