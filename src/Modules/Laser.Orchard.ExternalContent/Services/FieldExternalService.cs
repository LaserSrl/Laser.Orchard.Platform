using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Numerics;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using Laser.Orchard.ExternalContent.Settings;
using Laser.Orchard.Commons.Helpers;
using Orchard.Caching.Services;
using System.Web.Script.Serialization;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;
using Laser.Orchard.StartupConfig.Exceptions;
using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using System.Diagnostics;
using System.Text;

namespace Laser.Orchard.ExternalContent.Services {

    public interface IFieldExternalService : IDependency {

        dynamic GetContentfromField(Dictionary<string, object> contesto, string field, string nomexlst, FieldExternalSetting settings, string contentType = "", HttpVerbOptions httpMethod = HttpVerbOptions.GET, HttpDataTypeOptions httpDataType = HttpDataTypeOptions.JSON, string additionalHeadersText = "", string bodyRequest = "");

        string GetUrl(Dictionary<string, object> contesto, string externalUrl);

        void ScheduleNextTask(Int32 minute, ContentItem ci);
    }

    public class ExtensionObject {

        public string HtmlEncode(string input) {
            return System.Web.HttpUtility.HtmlEncode(input);
        }

        public string HtmlDecode(string input) {
            return (System.Web.HttpUtility.HtmlDecode(input)).Replace("\t", " ");
        }

        public string GuidToId(string input) {
            BigInteger huge = BigInteger.Parse('0' + input.Replace("-", "").Replace(" ", ""), NumberStyles.AllowHexSpecifier);
            return (huge + 5000000).ToString();
        }

        public string ToTitleCase(string input) {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            return myTI.ToTitleCase(input);
        }
    }

    public class FieldExternalService : IFieldExternalService {
        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSetting;
        private readonly IWorkContextAccessor _workContext;
        private readonly ICacheStorageProvider _cacheStorageProvider;
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private const string TaskType = "FieldExternalTask";
        private readonly IRazorTemplateManager _razorTemplateManager;

        public ILogger Logger { get; set; }

        public FieldExternalService(
            ITokenizer tokenizer
            , ShellSettings shellSetting
            , IWorkContextAccessor workContext
            , IOrchardServices orchardServices
            , IScheduledTaskManager scheduledTaskManager
            , IRazorTemplateManager razorTemplateManager
            ) {
            _tokenizer = tokenizer;
            _shellSetting = shellSetting;
            _workContext = workContext;
            Logger = NullLogger.Instance;
            _razorTemplateManager = razorTemplateManager;
            _orchardServices = orchardServices;
            if (_orchardServices.WorkContext != null) {
                _orchardServices.WorkContext.TryResolve<ICacheStorageProvider>(out _cacheStorageProvider);
            }
            _scheduledTaskManager = scheduledTaskManager;
        }

        public void ScheduleNextTask(Int32 minute, ContentItem ci) {
            if (minute > 0) {
                DateTime date = DateTime.UtcNow.AddMinutes(minute);
                _scheduledTaskManager.CreateTask(TaskType, date, ci);
            }
        }

        public string GetUrl(Dictionary<string, object> contesto, string externalUrl) {
            bool threatUrlAsString = false;
            string pureCleanString, finalUrl;
            var tokenizedzedUrl = _tokenizer.Replace(externalUrl, contesto, new ReplaceOptions { Encoding = ReplaceOptions.UrlEncode });
            pureCleanString = tokenizedzedUrl = tokenizedzedUrl.Replace("+", "%20");

            threatUrlAsString = !tokenizedzedUrl.StartsWith("http");
            if (threatUrlAsString) { // gestisco il caso in cui l'URl dell'externalField sia in realtà una stringa
                tokenizedzedUrl = String.Format("http://{0}/{1}/{2}", _shellSetting.RequestUrlHost ?? "www.fakedomain.com", _shellSetting.RequestUrlPrefix ?? "", tokenizedzedUrl ?? "");
            }
            Uri tokenizedzedUri;
            try {
                tokenizedzedUri = new Uri(tokenizedzedUrl);
            }
            catch {
                // gestisco il caso in cui passo un'url e non i parametri di un'url
                tokenizedzedUrl = _tokenizer.Replace(externalUrl, contesto, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
                tokenizedzedUri = new Uri(tokenizedzedUrl);
            }
            if (threatUrlAsString) {
                finalUrl = pureCleanString.Split('?')[0];
            }
            else {
                finalUrl = String.Format("{0}{1}{2}{3}", tokenizedzedUri.Scheme, Uri.SchemeDelimiter, tokenizedzedUri.Authority, tokenizedzedUri.AbsolutePath);
            }
            var queryStringParameters = tokenizedzedUri.Query.Split('&');
            var i = 0;
            foreach (var item in queryStringParameters) {
                if (!item.Trim().EndsWith("=")) {
                    finalUrl += ((i == 0 ? "?" : "&") + item.Replace("?", ""));
                    i++;
                }
            }

            if (finalUrl.Count(f => f == '?') == 1 && finalUrl[finalUrl.Length - 1] == '?')
                finalUrl = finalUrl.TrimEnd('?'); //delete the last ? if there aren t parameter in query string

            return finalUrl;
        }
        private string GetHeadersText(Dictionary<string, object> contesto, string additionalHeadersText) {
            return _tokenizer.Replace(additionalHeadersText, contesto, new ReplaceOptions() { Encoding = ReplaceOptions.NoEncode });
        }
        private JObject jsonflusher(JObject jsonObject) {
            JObject newJsonObject = new JObject();
            JProperty property;
            foreach (var token in jsonObject.Children()) {
                if (token != null) {
                    property = (JProperty)token;
                    if (property.Value.Children().Count() == 0)
                        newJsonObject.Add(property.Name.Replace(" ", ""), property.Value);
                    else if (property.Value.GetType().Name == "JArray") {
                        JArray myjarray = new JArray();
                        foreach (var arr in property.Value) {
                            if (arr.ToString() != "[]") {
                                if (arr.GetType().Name == "JValue")
                                    myjarray.Add(arr);
                                else
                                    myjarray.Add(jsonflusher((JObject)arr));
                            }
                        }
                        newJsonObject.Add(property.Name, myjarray);
                    }
                    else if (property.Value.GetType().Name == "JObject") {
                        newJsonObject.Add(property.Name.Replace(" ", ""), jsonflusher((JObject)property.Value));
                    }
                }
            }
            return newJsonObject;
        }

        public dynamic GetContentfromField(Dictionary<string, object> contesto, string externalUrl, string nomexlst, FieldExternalSetting settings, string contentType = "", HttpVerbOptions httpMethod = HttpVerbOptions.GET, HttpDataTypeOptions httpDataType = HttpDataTypeOptions.JSON, string tokenizedAdditionalHeadersText = "", string bodyRequest = "") {
            dynamic ci = null;
            string UrlToGet = "";
            string additionalHeadersText = "";
            string prefix = _shellSetting.Name + "_";
            try {
                UrlToGet = GetUrl(contesto, externalUrl);
                additionalHeadersText = GetHeadersText(contesto, tokenizedAdditionalHeadersText);
                string chiavecache = UrlToGet;
                chiavecache = Path.GetInvalidFileNameChars().Aggregate(chiavecache, (current, c) => current.Replace(c.ToString(), string.Empty));
                chiavecache = chiavecache.Replace('&', '_');
                string chiavedate = prefix + "Date_" + chiavecache;
                chiavecache = prefix + chiavecache;
                dynamic ciDate = _cacheStorageProvider.Get<object>(chiavedate);
                if (settings.CacheMinute > 0) {
                    ci = _cacheStorageProvider.Get<object>(chiavecache);
                }
                if (ci == null || ciDate == null) {
                    string certPath;
                    string webpagecontent;
                    if (settings.CertificateRequired && !String.IsNullOrWhiteSpace(settings.CerticateFileName)) {
                        certPath = String.Format(HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\ExternalFields\{0}", settings.CerticateFileName);
                        if (File.Exists(certPath)) {
                            webpagecontent = GetHttpPage(UrlToGet, httpMethod, httpDataType, additionalHeadersText, bodyRequest, certPath, settings.CertificatePrivateKey.DecryptString(_shellSetting.EncryptionKey)).Trim();
                        }
                        else {
                            throw new Exception(String.Format("File \"{0}\" not found! Upload certificate via FTP.", certPath));
                        }
                    }
                    else {
                        if (settings.DataType == OriginData.Executable) {
                            string filename = HostingEnvironment.MapPath("~/") + @"App_Code\" + externalUrl.Substring(0, externalUrl.IndexOf(".exe") + 4);
                            if (!File.Exists(filename)) {
                                throw new Exception(String.Format("File \"{0}\" not found!", filename));
                            }
                            var tmptokenizedzedUrl = _tokenizer.Replace(externalUrl, contesto);

                            var StartInfo = new ProcessStartInfo {
                                FileName = filename,
                                Arguments = tmptokenizedzedUrl.Substring(tmptokenizedzedUrl.IndexOf(".exe") + 5),
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true,
                                StandardOutputEncoding = Encoding.UTF8
                            };
                            var versionInfo = FileVersionInfo.GetVersionInfo(filename);
                            if (versionInfo.CompanyName != "Laser Group") {
                                throw new Exception(string.Format("ExternalExe {0}  has not correct CompanyName", filename));
                            }
                            else {
                                using (var proc = Process.Start(StartInfo)) {
                                    webpagecontent = proc.StandardOutput.ReadToEnd();
                                    string err = proc.StandardError.ReadToEnd();
                                    if (!string.IsNullOrEmpty(err)) {
                                        Logger.Error(string.Format("ExternalExe {0}  : {1}", externalUrl, err));
                                    }
                                    proc.WaitForExit();
                                };
                            }
                        }
                        else {
                            webpagecontent = GetHttpPage(UrlToGet, httpMethod, httpDataType, additionalHeadersText, bodyRequest).Trim();
                        }
                    }
                    if (!webpagecontent.StartsWith("<")) {
                        if (webpagecontent.StartsWith("[")) {
                            webpagecontent = String.Concat("{\"", nomexlst, "List", "\":", webpagecontent, "}");
                        }

                        if (webpagecontent.Trim() == "") {
                            // fix json vuoto
                            webpagecontent = "{}";
                        }

                        JObject jsonObject = JObject.Parse(webpagecontent);
                        JObject newJsonObject = new JObject();
                        newJsonObject = jsonflusher(jsonObject);
                        webpagecontent = newJsonObject.ToString();
                        XmlDocument newdoc = new XmlDocument();
                        newdoc = JsonConvert.DeserializeXmlNode(webpagecontent, "root");
                        correggiXML(newdoc);
                        webpagecontent = newdoc.InnerXml;
                    }

                    // fix json vuoto
                    if (webpagecontent == "<root />") {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(webpagecontent);

                        XmlNode xmlNode = xmlDoc.DocumentElement;
                        xmlNode = XmlWithJsonArrayTag(xmlNode, xmlDoc);

                        string JsonVuoto = JsonConvert.SerializeXmlNode(xmlNode);

                        JavaScriptSerializer ser = new JavaScriptSerializer() {
                            MaxJsonLength = Int32.MaxValue
                        };
                        dynamic dynamiccontent_tmp = ser.Deserialize(JsonVuoto, typeof(object));
                        ci = new DynamicJsonObject(dynamiccontent_tmp as IDictionary<string, object>);
                    }
                    else {
                        dynamic mycache = null;
                        Dictionary<string, object> dvb = new Dictionary<string, object>();
                        if (!string.IsNullOrEmpty(settings.CacheInput)) {
                            string inputcache = _tokenizer.Replace(settings.CacheInput, contesto);
                            mycache = _cacheStorageProvider.Get<object>(inputcache);
                            if (mycache == null) {
                                if (File.Exists(String.Format(HostingEnvironment.MapPath("~/") + "App_Data/Cache/" + inputcache))) {
                                    string filecontent = File.ReadAllText(String.Format(HostingEnvironment.MapPath("~/") + "App_Data/Cache/" + inputcache));
                                    mycache = JsonConvert.DeserializeObject(filecontent);
                                    _cacheStorageProvider.Put(inputcache, mycache);
                                }
                            }
                        }
                        if (mycache != null) {
                            XmlDocument xml = JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(mycache));
                            dvb.Add("CachedData", xml);
                        }
                        else {
                            dvb.Add("CachedData", new XmlDocument());
                        }
                        dvb.Add("externalUrl", UrlToGet);
                        dvb.Add("OrchardServices", _orchardServices);
                        ci = RazorTransform(webpagecontent.Replace(" xmlns=\"\"", ""), nomexlst, contentType, dvb);

                        _cacheStorageProvider.Remove(chiavecache);
                        _cacheStorageProvider.Remove(chiavedate);
                        if (settings.CacheMinute > 0) {
                            _cacheStorageProvider.Put(chiavecache, (object)ci);

                            // Use TimeSpan constructor to specify:
                            // ... Days, hours, minutes, seconds, milliseconds.
                            _cacheStorageProvider.Put(chiavedate, new { When = DateTime.UtcNow }, new TimeSpan(0, 0, settings.CacheMinute, 0, 0));
                        }
                        if (settings.CacheToFileSystem) {
                            if (!Directory.Exists(HostingEnvironment.MapPath("~/") + "App_Data/Cache"))
                                Directory.CreateDirectory(HostingEnvironment.MapPath("~/") + "App_Data/Cache");
                            using (StreamWriter sw = File.CreateText(String.Format(HostingEnvironment.MapPath("~/") + "App_Data/Cache/" + chiavecache))) {
                                sw.WriteLine(JsonConvert.SerializeObject(ci));//, Jsettings));// new JsonSerializerSettings {  EmptyArrayHandling = EmptyArrayHandling.Set }));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, UrlToGet);
                throw new ExternalFieldRemoteException();
            }
            return (ci);
        }

        public List<XmlNode> listanodi = new List<XmlNode>();

        private void correggiXML(XmlDocument xml) {
            foreach (XmlNode nodo in xml.ChildNodes) {
                doiteratenode(nodo, xml);
            }
            foreach (XmlNode sottonodo in listanodi) {
                int n;
                bool isNumeric = int.TryParse(sottonodo.Name, out n);
                if (isNumeric) {
                    RenameNode(xml, sottonodo, "lasernumeric");
                }
                if (sottonodo.Name.ToLower() == "description" || sottonodo.Name.ToLower() == "abstract" || sottonodo.Name.ToLower() == "extension") {
                    RenameNode(xml, sottonodo, sottonodo.Name + "text");
                }
            }
        }

        private void doiteratenode(XmlNode nodo, XmlDocument xml) {
            foreach (XmlNode sottonodo in nodo.ChildNodes) {
                int n;
                bool isNumeric = int.TryParse(sottonodo.Name, out n);
                if (isNumeric || sottonodo.Name.ToLower() == "description" || sottonodo.Name.ToLower() == "abstract" || sottonodo.Name.ToLower() == "extension") {
                    listanodi.Add(sottonodo);
                    // RenameNode(xml, sottonodo, "lasernumeric");
                }
                doiteratenode(sottonodo, xml);
            }
        }

        private void RenameNode(XmlDocument doc, XmlNode e, string newName) {
            XmlNode newNode = doc.CreateNode(e.NodeType, newName, null);
            while (e.HasChildNodes) {
                newNode.AppendChild(e.FirstChild);
            }
            XmlAttributeCollection ac = e.Attributes;
            while (ac.Count > 0) {
                newNode.Attributes.Append(ac[0]);
            }
            XmlNode parent = e.ParentNode;
            parent.ReplaceChild(newNode, e);
        }

        private dynamic RazorTransform(string xmlpage, string xsltname, string contentType = "", Dictionary<string, object> dvb = null) {
            string output = "";
            string myfile = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + contentType + xsltname + ".cshtml";
            if (!System.IO.File.Exists(myfile)) {
                myfile = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + xsltname + ".cshtml";
            }

            if (System.IO.File.Exists(myfile)) {
                string key = myfile;
                DateTime d = System.IO.File.GetLastWriteTime(myfile);
                key += d.ToShortDateString() + d.ToLongTimeString();
                string mytemplate = File.ReadAllText(myfile);
                string myfile2 = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\common.cshtml";
                if (System.IO.File.Exists(myfile2)) {
                    mytemplate = File.ReadAllText(myfile2) + mytemplate; ;
                }
                if (!string.IsNullOrEmpty(mytemplate)) {
                    var docwww = XDocument.Parse(xmlpage);
                    string result = _razorTemplateManager.RunString(key, mytemplate, docwww, dvb, null);
                    output = result.Replace("\r\n", "");
                }
                else
                    output = "";
                while (output.StartsWith("\t")) {
                    output = output.Substring(1);
                }

                // Fix per non generare eccezione se output è uguale a ""
                string xml = "<root />";
                if (output != "")
                    xml = RemoveAllNamespaces(output);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNode newNode = doc.DocumentElement;
                newNode = XmlWithJsonArrayTag(newNode, doc);

                string JsonData = JsonConvert.SerializeXmlNode(newNode);
                JsonData = JsonData.Replace(",\"\"]}", "]}"); // aggiunto perchè nella nuova versione di newtonjson i nodi xml vuoti non vengono piu tradotti in null ma in ""
                JsonData = JsonData.Replace(",{}]}", "]}");

                JsonData = JsonData.Replace("\":lasernumericlasernumeric:\"", "null");
                JsonData = JsonData.Replace("\":lasernumeric", "");
                JsonData = JsonData.Replace("lasernumeric:\"", "");
                JsonData = JsonData.Replace("\":laserbooleanlaserboolean:\"", "null");
                JsonData = JsonData.Replace("\":laserboolean", "");
                JsonData = JsonData.Replace("laserboolean:\"", "");
                JsonData = JsonData.Replace(@"\r\n", "");
                JsonData = JsonData.Replace("\":laserDatelaserDate:\"", "null");
                JsonData = JsonData.Replace("\":laserDate", "\"\\/Date(");
                JsonData = JsonData.Replace("laserDate:\"", ")\\/\"");


                JavaScriptSerializer ser = new JavaScriptSerializer() {
                    MaxJsonLength = Int32.MaxValue
                };
                dynamic dynamiccontent_tmp = ser.Deserialize(JsonData, typeof(object));
                dynamic dynamiccontent = new DynamicJsonObject(dynamiccontent_tmp as IDictionary<string, object>);

                return dynamiccontent;
            }
            else {
                return XsltTransform(xmlpage, xsltname, contentType);
            }
        }

        private XmlNode XmlWithJsonArrayTag(XmlNode xn, XmlDocument doc) {
            bool ForceChildBeArray = false;
            if (xn.ChildNodes.Count > 1) {
                if (xn.ChildNodes[0].Name == xn.ChildNodes[1].Name && xn.ChildNodes[0].Name != "ToRemove") {
                    ForceChildBeArray = true;
                }
            }

            for (Int32 i = 0; i < xn.ChildNodes.Count; i++) {
                if (ForceChildBeArray) {
                    XmlAttribute xattr = doc.CreateAttribute("json", "Array", "http://james.newtonking.com/projects/json");
                    xattr.Value = "true";
                    xn.ChildNodes[i].Attributes.Append(xattr);
                }
                if (xn.ChildNodes[i].HasChildNodes) {
                    XmlNode childnode = XmlWithJsonArrayTag(xn.ChildNodes[i], doc).Clone();
                    if (!string.IsNullOrEmpty(childnode.InnerText)) {
                        xn.InsertBefore(childnode, xn.ChildNodes[i]);
                    }
                    xn.ChildNodes[i].ParentNode.RemoveChild(xn.ChildNodes[i]);
                }
            }
            return xn;
        }

        private dynamic XsltTransform(string xmlpage, string xsltname, string contentType = "") {
            string output = "", myXmlFileMoreSpecific, myXmlFileLessSpecific, myXmlFile;
            var namespaces = this.GetType().FullName.Split('.').AsEnumerable();
            namespaces = namespaces.Except(new string[] { this.GetType().Name });
            namespaces = namespaces.Except(new string[] { namespaces.Last() });
            var area = string.Join(".", namespaces);
            // se esiste un xslt chiamato {ContentType}.{FieldName}.xslt ha priorità rispetto agli altri
            myXmlFile = myXmlFileLessSpecific = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + xsltname + ".xslt";
            myXmlFileMoreSpecific = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + contentType + "." + xsltname + ".xslt";
            if (File.Exists(myXmlFileMoreSpecific)) {
                myXmlFile = myXmlFileMoreSpecific;
            }

            if (File.Exists(myXmlFile)) {
                XmlReader myXPathDoc = XmlReader.Create(new StringReader(xmlpage));
                myXPathDoc.Read();
                XsltArgumentList argsList = new XsltArgumentList();

                argsList.AddExtensionObject("my:HttpUtility", new ExtensionObject());

                string cult = _workContext.GetContext().CurrentCulture;
                if (String.IsNullOrEmpty(cult))
                    cult = "it";
                else
                    cult = cult.Substring(0, 2);

                argsList.AddParam("LinguaParameter", "", cult);

                var allrequest = _workContext.GetContext().HttpContext.Request.QueryString.Keys;

                for (var i = 0; i < allrequest.Count; i++) {
                    string _key = allrequest[i];
                    string _value = _workContext.GetContext().HttpContext.Request.QueryString[_key].ToString();
                    argsList.AddParam(_key.ToLower().Trim(), "", _value);
                }

                XsltSettings settings = new XsltSettings();
                settings.EnableScript = true;

                XslCompiledTransform myXslTrans;
                var enableXsltDebug = false;
#if DEBUG
                enableXsltDebug = true;
#endif
                myXslTrans = new XslCompiledTransform(enableXsltDebug);
                myXslTrans.Load(myXmlFile, settings, new XmlUrlResolver());

                StringWriter sw = new StringWriter();
                XmlWriter xmlWriter = new XmlTextWriter(sw);
                myXslTrans.Transform(myXPathDoc, argsList, xmlWriter);

                output = sw.ToString();
            }
            else {
                output = xmlpage;
            }
            string xml = RemoveAllNamespaces(output);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode newNode = doc.DocumentElement;
            string JsonData = JsonConvert.SerializeXmlNode(newNode);
            JsonData = JsonData.Replace(",\"\"]}", "]}"); // aggiunto perchè nella nuova versione di newtonjson i nodi xml vuoti non vengono piu tradotti in "" ma in null
            JsonData = JsonData.Replace("\":lasernumeric", "");
            JsonData = JsonData.Replace("lasernumeric:\"", "");
            JsonData = JsonData.Replace("\":laserboolean", "");
            JsonData = JsonData.Replace("laserboolean:\"", "");
            JsonData = JsonData.Replace(@"\r\n", "");
            JsonData = JsonData.Replace("\":laserDate", "\"\\/Date(");
            JsonData = JsonData.Replace("laserDate:\"", ")\\/\"");
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = Int32.MaxValue;
            dynamic dynamiccontent_tmp = ser.Deserialize(JsonData, typeof(object));
            dynamic dynamiccontent = new DynamicJsonObject(dynamiccontent_tmp);

            return dynamiccontent;
        }

        private static string GetHttpPage(string uri, HttpVerbOptions httpMethod, HttpDataTypeOptions httpDataType, string additionalHeadersText, string bodyRequest, string certificatePath = null, string privateKey = null) {
            Stream dataStream = null;
            String strResult;
            WebResponse objResponse;
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(uri);
            if (certificatePath != null) {
                var bytes = File.ReadAllBytes(certificatePath);
                X509Certificate2 cert = new X509Certificate2(bytes, privateKey);
                objRequest.ClientCertificates.Add(cert);
            }
            objRequest.Headers.Add(HttpRequestHeader.ContentEncoding, "gzip");
            SetAdditionalHeaders(objRequest, additionalHeadersText);

            objRequest.Method = httpMethod.ToString();

            // valore di default del content type
            objRequest.ContentType = "application/x-www-form-urlencoded";

            if (httpMethod == HttpVerbOptions.POST) {
                if (httpDataType == HttpDataTypeOptions.JSON) {
                    // JSON
                    objRequest.ContentType = "application/json; charset=utf-8";
                }

                // body del post
                byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(bodyRequest);
                dataStream = objRequest.GetRequestStream();
                dataStream.Write(buffer, 0, buffer.Length);
                dataStream.Close();
            }

            objRequest.PreAuthenticate = false;
            objResponse = objRequest.GetResponse();
            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream())) {
                strResult = sr.ReadToEnd();
                sr.Close();
            }
            return strResult;
        }
        private static void SetAdditionalHeaders(HttpWebRequest request, string additionalHeadersText) {
            if (string.IsNullOrWhiteSpace(additionalHeadersText) == false) {
                var rows = additionalHeadersText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var row in rows) {
                    var idx = row.IndexOf(':');
                    //a line starting with ':' (idx == 0) is not a valid hader
                    if (idx > 0) {
                        var headerName = row.Substring(0, idx).Trim();
                        var headerVal = row.Substring(idx + 1).Trim();
                        if (string.IsNullOrWhiteSpace(headerName) == false) {
                            request.Headers.Set(headerName, headerVal);
                        }
                    }
                }
            }
        }
        private static string RemoveAllNamespaces(string xmlDocument) {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));
            return xmlDocumentWithoutNs.ToString();
        }

        //Core recursion function
        private static XElement RemoveAllNamespaces(XElement xmlDocument) {
            if (!xmlDocument.HasElements) {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }
    }

    public class DynamicXml : DynamicObject {
        private XElement _root;

        private DynamicXml(XElement root) {
            _root = root;
        }

        public static DynamicXml Parse(string xmlString) {
            return new DynamicXml(XDocument.Parse(xmlString).Root);
        }

        public static DynamicXml Load(string filename) {
            return new DynamicXml(XDocument.Load(filename).Root);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = null;

            var att = _root.Attribute(binder.Name);
            if (att != null) {
                result = att.Value;
                return true;
            }

            var nodes = _root.Elements(binder.Name);
            if (nodes.Count() > 1) {
                result = nodes.Select(n => new DynamicXml(n)).ToList();
                return true;
            }

            var node = _root.Element(binder.Name);
            if (node != null) {
                if (node.HasElements) {
                    result = new DynamicXml(node);
                }
                else {
                    result = node.Value;
                }
                return true;
            }

            return true;
        }
    }
}