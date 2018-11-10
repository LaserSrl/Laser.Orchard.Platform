using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using System.Xml.Schema;

namespace Laser.Orchard.WebServices.ViewModels {

    // todo completare i casi, questi li ho presi da qui http://api.turismotorino.org/v1_01/events?type=bydate&date=08/11/2014
    public enum Lingue { it, en };

    public enum Frequency { daily }; //TODO aggiungere i tipi

    [Serializable]
    [XmlRoot("Event", Namespace = "")]
    [XmlTypeAttribute(AnonymousType = true, Namespace = "")]
    public class Eventi015VM {
        //  [XmlAttributeAttribute(AttributeName = "schemaLocation", Namespace = "")]
        //public string xsiSchemaLocation = "http://www.turismotorino.org/schema/e015/v1_1/E015_Glossario_Eventi_POI_Itinerari_v1_1.xsd";

        public string DataSource { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public List<InLingua> Abstract { get; set; }

        [XmlElement("Description")]
        public List<InLinguaCData> Description { get; set; }

        [XmlElement("Category")]
        public List<InLingua> Category { get; set; }

        [XmlElement("Scope")]
        public List<InLingua> Scope { get; set; }

        [XmlElement("Coverage")]
        public List<InLingua> Coverage { get; set; }

        [XmlElement("Contact")]
        public List<Contact> Contact { get; set; }

        [XmlElement("PriceInfo")]
        public List<InLingua> PriceInfo { get; set; }

        





        /// <summary>
        /// caso di Venue
        /// </summary>
        [XmlElement("Geolocation", IsNullable = true)]
        public Geolocation Geolocation { get; set; }

[XmlElement("MediaResource")]
        public List<MediaResource> MediaResource { get; set; }
        //   [XmlElement("ItineraryStep", IsNullable = true)]
        //public Venue ItineraryStep { get; set; }

        [XmlElement("Schedule")]
        public Schedule Schedule { get; set; }

        /// <summary>
        /// caso di Event
        /// </summary>
        [XmlElement("Venue", IsNullable = true)]
        public List<Venue> Venue { get; set; }
        [XmlIgnore]
        public string tipoestrazione { get; set; }



        public Eventi015VM() {
            this.Abstract = new List<InLingua>();
            this.Description = new List<InLinguaCData>();
            this.Category = new List<InLingua>();
            this.Scope = new List<InLingua>();
            this.Coverage = new List<InLingua>();
            this.Contact = new List<Contact>();
            this.PriceInfo = new List<InLingua>();
            this.Schedule = new Schedule();
            this.Venue = new List<Venue>();
            this.MediaResource = new List<MediaResource>();
            this.Geolocation = new Geolocation();
            //       this.ItineraryStep = new Venue();
        }

        public string Serialize {
            get {
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                // namespaces.Add(string.Empty, " http://www.turismotorino.org/schema/e015/v1_1/E015_Glossario_Eventi_POI_Itinerari_v1_1.xsd");

                XmlSerializer x = new XmlSerializer(this.GetType(), new XmlRootAttribute(this.tipoestrazione));
                using (MemoryStream memStream = new MemoryStream()) {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = Encoding.UTF8;
                    // settings.Indent = true;
                    //settings.IndentChars = "\t";
                    //settings.NewLineChars = Environment.NewLine;
                    //settings.ConformanceLevel = ConformanceLevel.Document;
                    settings.OmitXmlDeclaration = true;
                    using (XmlWriter writer = XmlTextWriter.Create(memStream, settings)) {
                        x.Serialize(writer, this, namespaces);
                    }
                    string xml = Encoding.UTF8.GetString(memStream.ToArray());
                    //XElement doc = XElement.Parse(xml);
                    //doc.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    //return doc.ToString();
                    if (xml.StartsWith("<Event>"))
                        xml = "<Event xsi:type=\"ns:eventType\">" + xml.Substring(8);
                    if (xml.StartsWith("<Venue>"))
                        xml = "<Venue xsi:type=\"ns:venueType\">" + xml.Substring(8);
                    if (xml.StartsWith("<Itinerary>"))
                        xml = "<Itinerary xsi:type=\"ns:itineraryType\">" + xml.Substring(12);
                    return xml.Replace("<Venue><Geolocation><Geocoordinates /></Geolocation></Venue>", "")
                        .Replace("<Geolocation><Geocoordinates /></Geolocation>", "")
                        .Replace("<Abstract />", "")
                        .Replace("<Schedule />", "");

                }
            }
        }

    }

    public class MediaResource {

        [XmlElement("Name")]
        public List<InLingua> Name { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public MediaResource() {
            this.Name = new List<InLingua>();
        }

        [XmlIgnore]
        public string MediaUrl { get; set; }
    }





    public class InLinguaCData {


        [XmlAttribute("xml:lang")]
        public Lingue Lingua { get; set; }

        [XmlIgnore]
        public string _message { get; set; }


        //[XmlText]
        //public string Valore {
        //    get {
        //        XCData cdata = new XCData(_message);
        //        return cdata.ToString();
        //    }
        //    set { _message = value; }
        //}



        //[XmlText]
        //public XmlCDataSection Valore {//XmlCDataSection XmlElement("CDataElement"),
        //    get {
        //        XmlDocument doc = new XmlDocument();
        //        return doc.CreateCDataSection(_message);
        //    }
        //    set {
        //        _message = value.Value;
        //    }
        //}

        [XmlText]
        public XmlNode[] Valore {//XmlCDataSection XmlElement("CDataElement"),
            get {

                var dummy = new XmlDocument();
                return new XmlNode[] { dummy.CreateCDataSection(_message.Replace("\r\n","")) };
            }
            set {
                if (value == null) {
                    _message = null;
                    return;
                }

                if (value.Length != 1) {
                    throw new InvalidOperationException(
                        String.Format(
                            "Invalid array length {0}", value.Length));
                }

                _message = value[0].Value;
            }
        }



    }



    //public class ActionsCDataField : IXmlSerializable {
    //    public List<Action> Actions { get; set; }

    //    public ActionsCDataField() {
    //        Actions = new List<Action>();
    //    }

    //    public XmlSchema GetSchema() {
    //        return null;
    //    }

    //    public void WriteXml(XmlWriter w) {
    //        foreach (var item in Actions) {
    //            w.WriteStartElement("Action");
    //            w.WriteAttributeString("Type", item.Type);
    //            w.WriteCData(item.InnerText);
    //            w.WriteEndElement();
    //            w.WriteString("\r\n");
    //        }
    //    }

    //    public void ReadXml(XmlReader r) {
    //        XmlDocument xDoc = new XmlDocument();
    //        xDoc.Load(r);

    //        XmlNodeList nodes = xDoc.GetElementsByTagName("Action");
    //        if (nodes != null && nodes.Count > 0) {
    //            foreach (XmlElement node in nodes) {
    //                Action a = new Action();
    //                a.Type = node.GetAttribute("Type");
    //                a.InnerText = node.InnerXml;
    //                if (a.InnerText != null && a.InnerText.StartsWith("<![CDATA[") && a.InnerText.EndsWith("]]>"))
    //                    a.InnerText = a.InnerText.Substring("<![CDATA[".Length, a.InnerText.Length - "<![CDATA[]]>".Length);

    //                Actions.Add(a);
    //            }
    //        }
    //    }
    //}
    //public class Action {
    //    public String Type { get; set; }
    //    public String InnerText { get; set; }
    //}

    public class InLingua {

        [XmlAttribute("xml:lang")]
        public Lingue Lingua { get; set; }

        [XmlText]
        public string Valore { get; set; }
    }

    public class Contact {

        public string String { get; set; }

        public string Type { get; set; }
    }

    public class Schedule {

        [XmlElement("Recurrence")]
        public List<Recurrence> Recurrence { get; set; }

        [XmlElement("Occurrence")]
        public List<Occurrence> Occurrence { get; set; }

        public Schedule() {
            this.Recurrence = new List<Recurrence>();
            this.Occurrence = new List<Occurrence>();
        }
    }

    public class Occurrence {

        public Range StartEnd { get; set; }

        public Occurrence() {
            this.StartEnd = new Range();
        }
    }

    public class Recurrence {

        public Range Range { get; set; }

        public Frequency Frequency { get; set; }

        public StartEnd StartEnd { get; set; }
    }

    public class Range {

        [XmlIgnore]
        public DateTime StartDateTime { get; set; }

        [XmlIgnore]
        public DateTime EndDateTime { get; set; }

        [XmlElement("StartDateTime")]
        public string StartDateTimeString {
            get { return this.StartDateTime.ToString("yyyy-MM-ddTHH':'mm':'sszzz"); }
            set { this.StartDateTime = DateTime.Parse(value); }
        }

        [XmlElement("EndDateTime")]
        public string EndDateTimeString {
            get { return this.EndDateTime.ToString("yyyy-MM-ddTHH':'mm':'sszzz"); }
            set { this.EndDateTime = DateTime.Parse(value); }
        }
    }

    public class StartEnd {

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class Venue {

        [XmlElement("Name")]
        public List<InLingua> Name { get; set; }

        [XmlElement("Category")]
        public List<InLingua> Category { get; set; }

        public Address Address { get; set; }

        public Geolocation Geolocation { get; set; }
        [XmlIgnore]
        public Int32 MasterId { get; set; }

        public Venue() {
            this.Name = new List<InLingua>();
            this.Geolocation = new Geolocation();
        }
    }

    public class Address {

        public string Street { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }
    }

    public class Geolocation {
        [XmlElement("Geocoordinates")]
        public Geocoordinates Geocoordinates { get; set; }

        public Geolocation() {
            this.Geocoordinates = new Geocoordinates();
        }
    }

    public class Geocoordinates {

        public string XCoord { get; set; }

        public string YCoord { get; set; }
    }
}