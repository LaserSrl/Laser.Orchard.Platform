using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Meteo.Models {
    public class MeteoPart : ContentPart {
        public MeteoPart() {
            _details = new List<MeteoInfo>();
        }
        internal string _provider { get; set; }
        internal DateTime _forecastDate { get; set; }
        internal string _situation { get; set; }
        internal IList<MeteoInfo> _details { get; set; }
        internal int _pressure { get; set; }

        public string Provider { get { return _provider; } }
        public DateTime ForecastDate { get { return _forecastDate; } }
        public string Situation { get { return _situation; } }
        public IList<MeteoInfo> Details { get { return _details; } }
        public int Pressure { get { return _pressure; } }
    }

    public class MeteoInfo : MeteoInfoDetails {
        public MeteoInfo() {
            _pSubInfos = new List<MeteoInfoDetails>();
        }

        internal IList<MeteoInfoDetails> _pSubInfos { get; set; }

        public IList<MeteoInfoDetails> SubInfos { get { return _pSubInfos; } }

    }

    public class MeteoInfoDetails {
        public MeteoInfoDetails() {
            _pTemperature = new List<Temperature>();
        }

        internal DateTime _pForecastDate { get; set; }
        internal string _pForecastDescription { get; set; }
        internal IList<Temperature> _pTemperature { get; set; }
        internal Single _pWindKmh { get; set; }
        internal string _pWindDescription { get; set; }
        internal Single _pHumidityPercentage { get; set; }
        internal string _pHuumidityDescription { get; set; }
        internal Single _pPressurePercentage { get; set; }
        internal string _pPressureDescription { get; set; }
        internal SingleRange _pFreezingLevel { get; set; }
        internal string _pFreezingLevelDescription { get; set; }
        internal string _pAdvertise { get; set; }
        internal int _pAccuracy { get; set; }
        internal string _pImageUrl { get; set; }

        public DateTime ForecastDate { get { return _pForecastDate; } }
        public string ForecastDescription { get { return _pForecastDescription; } }
        public IList<Temperature> Temperatures { get { return _pTemperature; } }
        public Single WindKmh { get { return _pWindKmh; } }
        public string WindDescription { get { return _pWindDescription; } }
        public Single HumidityPercentage { get { return _pHumidityPercentage; } }
        public string HuumidityDescription { get { return _pHuumidityDescription; } }
        public Single PressurePercentage { get { return _pPressurePercentage; } }
        public string PressureDescription { get { return _pPressureDescription; } }
        public SingleRange FreezingLevel { get { return _pFreezingLevel; } }
        public string FreezingLevelDescription { get { return _pFreezingLevelDescription; } }
        public string Advertise { get { return _pAdvertise; } }
        public int Accuracy { get { return _pAccuracy; } }
        public string ImageUrl { get { return _pImageUrl; } }
        /**/
    }
    public class SingleRange {

        internal Single _pMin { get; set; }
        internal Single _pMax { get; set; }

        public Single Min { get { return _pMin; } }
        public Single Max { get { return _pMax; } }
    }

    public class Temperature {
        internal int _pAltitude { get; set; }
        internal Single _pTemperatureDegree { get; set; }
        internal string _pTemperatureDescription { get; set; }

        public int Altitude { get { return _pAltitude; } }
        public Single TemperatureDegree { get { return _pTemperatureDegree; } }
        public string TemperatureDescription { get { return _pTemperatureDescription; } }
    }
}