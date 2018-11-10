using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;
using Laser.Orchard.BikeSharing.com.mobilityparc.www;
using System.Net;
using System.IO;
using Laser.Orchard.BikeSharing.ViewModels;
using Orchard.Logging;

namespace Laser.Orchard.BikeSharing.Services {
    public interface IBikeServices : IDependency {
        BikeStationInfos GetStationInfo(string StationName);

    }

    public class MobilityParcServices : IBikeServices {

        public MobilityParcServices() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public BikeStationInfos GetStationInfo(string StationName) {
            Laser.Orchard.BikeSharing.com.mobilityparc.www.StationOpWSService s = new StationOpWSService();
            Uri uriWs = new Uri(s.Url);
            CookieContainer cookieJar = new CookieContainer();
            HttpWebRequest request;
            WebResponse response = null;
            StreamReader reader = null; ;
            Stream dataStream;
            // get the page to retrieve the Session Cookie!
            try {
                request = (HttpWebRequest)WebRequest.Create(s.Url + "?wsdl");
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };

                request.CookieContainer = cookieJar;
                request.Credentials = CredentialCache.DefaultCredentials;
                response = request.GetResponse();
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                response.Close();
            } catch (Exception ex) {
                Logger.Error("MobilityParc: Unable to create Session Cookie. Originale Message: " + ex.Message);
                return null;
            } finally {
                // Display the content.
                // Clean up the streams and the response.
                reader = null;
                response = null;
                request = null;
            }
            // post credentials and retrieve cookie
            try {

                var loginUrl = uriWs.Scheme + "://" + uriWs.Host + "/j_spring_security_check";
                var requestBody = "language=fr&j_username=reveWs&j_password=Reve2014&Submit=Envoyer";
                request = (HttpWebRequest)WebRequest.Create(loginUrl);
                request.CookieContainer = cookieJar;
                request.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                response = request.GetResponse();
            } catch (Exception ex) {
                Logger.Error("MobilityParc: Unable to log into Bike Sharing provider. Originale Message: " + ex.Message);
                return null;

            } finally {
                request = null;
            }



            // call web-service

            s.CookieContainer = cookieJar;
            var result = s.retrieveBicycles(StationName);
            if (result.error == 1) {
                Logger.Error("MobilityParc: Unable to log into Bike Sharing provider. Session Expired.");
                return null;
            } else if (result.error == 4) {
                Logger.Error("MobilityParc: Unknown Bike Station \"" + StationName + "\"");
                return null;
            }
            var results = result.bikeWSResps.Select(sel => new Terminal {
                BikeUid = sel.bikeUid,
                BikeName = sel.bikeName,
                BatteryLevel = sel.batteryLevel,
                TerminalNumber = sel.terminal
            });
            return new BikeStationInfos {
                Terminals = results.ToList()
            };
        }
    }
}
