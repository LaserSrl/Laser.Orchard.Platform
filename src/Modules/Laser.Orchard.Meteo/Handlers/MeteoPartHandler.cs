using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml;
using Laser.Orchard.Meteo.Models;
using Orchard;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Meteo.Handlers {
    public class MeteoPartHandler : ContentHandler {

        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContext;

        public MeteoPartHandler(IOrchardServices orchardServices, IWorkContextAccessor workContext) {
            _orchardServices = orchardServices;
            _workContext = workContext;

            OnLoaded<MeteoPart>((context, part) => LoadMeteo(context, part));
        }

        protected void LoadMeteo(LoadContentContext context, MeteoPart part) {
            //base.Loaded(context);
            // TODO: create settings dfor theese values
            var providerUrl = "";
            var provider = MeteoProviders.RegioneVda;
            //var part = context.ContentItem.Parts.Where(w => w.PartDefinition.Name == typeof(MeteoPart).Name).Cast<MeteoPart>().SingleOrDefault();
            if (part == null) return;
            if (provider == MeteoProviders.RegioneVda) {
                providerUrl = "http://www.regione.vda.it/territorio/centrofunzionale/meteo/dati2013/bollettino_meteo/bollettino_{0}.xml";
                var wcontext = _workContext.GetContext();
                var culture = "";
                try { 
                    culture = wcontext.CurrentSite.SiteCulture; 
                    culture = wcontext.CurrentCulture; 
                } catch { }
                var linguaRegione = "i";
                if (culture != null) {
                    linguaRegione = culture.Substring(0, 1).ToLower();
                    if (!"ife".Contains(linguaRegione)) {
                        linguaRegione = "i";
                    }

                }
                // Funzione per italiano, francese e inglese
                GetFromRegione(String.Format(providerUrl, linguaRegione), linguaRegione, ref part);
            } else {
                throw new ApplicationException("Unsupported Provider!");
            }
        }

        private void GetFromRegione(string url, string linguaRegione, ref MeteoPart part) {
            var isMorning = DateTime.Now.Hour < 13;
            XmlDocument doc = new XmlDocument();
            doc.Load(url);
            part._provider = doc.GetElementsByTagName("ente")[0].InnerText;
            part._forecastDate = DateTime.ParseExact(doc.SelectNodes("bollettino/emissione")[0].InnerText, "dd/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            part._pressure = doc.SelectNodes("bollettino/pressione_rlm")[0] != null ? int.Parse(doc.SelectNodes("bollettino/pressione_rlm")[0].InnerText) : 0;
            part._situation = doc.SelectNodes("bollettino/situazione")[0] != null ? doc.SelectNodes("bollettino/situazione")[0].InnerText : null;
            var i = 0;
            var node = doc.SelectNodes("bollettino/giorno_" + (i + 1).ToString())[0];
            while (node != null) {
                try {
                    part._details.Add(new MeteoInfo {
                        _pForecastDate = DateTime.ParseExact(node.SelectNodes("data")[0].InnerText, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        _pForecastDescription = node.SelectNodes("previsione")[0].InnerText,
                        _pAdvertise = node.SelectNodes("segnalazioni")[0] != null ? node.SelectNodes("segnalazioni")[0].InnerText : null,
                        _pAccuracy = int.Parse(node.SelectNodes("idx_attend")[0] != null ? node.SelectNodes("idx_attend")[0].InnerText : "0"),
                        _pWindDescription = node.SelectNodes("venti")[0] != null ? node.SelectNodes("venti")[0].InnerText : null,
                        _pFreezingLevelDescription = node.SelectNodes("zero_termico")[0] != null ? node.SelectNodes("zero_termico")[0].InnerText : null,
                        _pTemperature = new List<Temperature> { 
                        new Temperature{ _pAltitude = 0,    _pTemperatureDescription =  node.SelectNodes("temperature")[0]!=null?node.SelectNodes("temperature")[0].InnerText:null},
                        new Temperature{ _pAltitude = 1500, _pTemperatureDescription =  node.SelectNodes("temperatura_1500")[0]!=null?node.SelectNodes("temperatura_1500")[0].InnerText:null},
                        new Temperature{ _pAltitude = 3000, _pTemperatureDescription =  node.SelectNodes("temperatura_3000")[0]!=null?node.SelectNodes("temperatura_3000")[0].InnerText:null},
                    },
                        _pPressureDescription = node.SelectNodes("pressione")[0] != null ? node.SelectNodes("pressione")[0].InnerText : null,
                        _pImageUrl = i <= 2 ? "http://www.regione.vda.it/territorio/centrofunzionale/meteo/dati2013/bollettino_meteo/valle_" + (i + 1) + "_" + linguaRegione + (isMorning ? "m" : "p") + "_s.png?" + DateTime.Now.ToShortDateString() : null,
                        _pSubInfos = i <= 2 ? new List<MeteoInfoDetails>() { 
                            new MeteoInfoDetails{ _pForecastDate = DateTime.ParseExact(node.SelectNodes("data")[0].InnerText+" 8.00", "yyyy-MM-dd H.mm", System.Globalization.CultureInfo.InvariantCulture), _pImageUrl="http://www.regione.vda.it/territorio/centrofunzionale/meteo/dati2013/bollettino_meteo/valle_"+(i+1)+"_"+linguaRegione+"m_s.png?"+DateTime.Now.ToShortDateString() },
                            new MeteoInfoDetails{ _pForecastDate = DateTime.ParseExact(node.SelectNodes("data")[0].InnerText+" 14.00", "yyyy-MM-dd H.mm", System.Globalization.CultureInfo.InvariantCulture), _pImageUrl="http://www.regione.vda.it/territorio/centrofunzionale/meteo/dati2013/bollettino_meteo/valle_"+(i+1)+"_"+linguaRegione+"p_s.png?"+DateTime.Now.ToShortDateString() },
                        } : null,
                    });


                    i++;
                    node = doc.SelectNodes("bollettino/giorno_" + (i + 1).ToString())[0];
                } catch {
                    break;
                }
            }
            doc = null;
        }
    }
}