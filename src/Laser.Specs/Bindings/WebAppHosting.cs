using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Castle.Core.Logging;
using HtmlAgilityPack;
using log4net.Appender;
using log4net.Core;
using NUnit.Framework;
using Laser.Specs.Hosting;
using TechTalk.SpecFlow;
using Path = Bleroy.FluentPath.Path;

namespace Laser.Specs.Bindings {
    [Binding]
    public class WebAppHosting {
        private WebHost _webHost;
        private RequestDetails _details;
        private HtmlDocument _doc;
        private MessageSink _messages;
        private static readonly Path _orchardTemp = Path.Get(System.IO.Path.GetTempPath()).Combine("Orchard.Specs");
        private ExtensionDeploymentOptions _moduleDeploymentOptions = ExtensionDeploymentOptions.CompiledAssembly;
        private DynamicCompilationOption _dynamicCompilationOption = DynamicCompilationOption.Enabled;

        public WebHost Host {
            get { return _webHost; }
        }

        public RequestDetails Details {
            get { return _details; }
            set { _details = value; }
        }

        [BeforeTestRun]
        public static void BeforeTestRun() {
            try { _orchardTemp.Delete(true).CreateDirectory(); } catch { }
        }

        [AfterTestRun]
        public static void AfterTestRun() {
            try {
                _orchardTemp.Delete(true); // <- try to clear any stragglers on the way out
            } catch { }
        }

        [BeforeScenario]
        public void CleanOutTheOldWebHost() {
            if (_webHost != null) {
                _webHost.Clean();
                _webHost = null;
            }
        }

        [AfterScenario]
        public void AfterScenario() {
            if (_webHost != null) {
                _webHost.Dispose();
            }
        }

        [Given(@"I have a clean site")]
        public void GivenIHaveACleanSite(string virtualDirectory = "/") {
            GivenIHaveACleanSiteBasedOn("Orchard.Web", virtualDirectory);
        }

        [Given(@"I have chosen to deploy modules as source files only")]
        public void GivenIHaveChosenToDeployModulesAsSourceFilesOnly() {
            _moduleDeploymentOptions = ExtensionDeploymentOptions.SourceCode;
        }

        [Given(@"I have chosen to load modules using dymamic compilation only")]
        public void GivenIHaveChosenToLoadModulesUsingDynamicComilationOnly() {
            _moduleDeploymentOptions = ExtensionDeploymentOptions.SourceCode;
            _dynamicCompilationOption = DynamicCompilationOption.Force;
        }

        [Given(@"I have chosen to load modules with dynamic compilation disabled")]
        public void GivenIHaveChosenToLoadModulesAsSourceFilesOnly() {
            _dynamicCompilationOption = DynamicCompilationOption.Disabled;
        }

        [Given(@"I have a clean site based on (.*)")]
        public void GivenIHaveACleanSiteBasedOn(string siteFolder) {
            GivenIHaveACleanSiteBasedOn(siteFolder, "/");
        }

        [Given(@"I have a clean site based on (.*) at ""(.*)""")]
        public void GivenIHaveACleanSiteBasedOn(string siteFolder, string virtualDirectory) {
            _webHost = new WebHost(_orchardTemp);
            Host.Initialize(siteFolder, virtualDirectory ?? "/", _dynamicCompilationOption);
            var shuttle = new Shuttle();
            Host.Execute(() => Executor(shuttle));
            _messages = shuttle._sink;
        }

        private static void Executor(Shuttle shuttle) {
            HostingTraceListener.SetHook(msg => shuttle._sink.Receive(msg));
        }

        private class CastleAppender : IAppender {
            public void Close() { }
            public string Name { get; set; }

            public void DoAppend(LoggingEvent loggingEvent) {
                var traceLoggerFactory = new TraceLoggerFactory();
                var logger = traceLoggerFactory.Create(loggingEvent.LoggerName);
                if (loggingEvent.Level <= Level.Debug)
                    logger.Debug(loggingEvent.RenderedMessage);
                else if (loggingEvent.Level <= Level.Info)
                    logger.Info(loggingEvent.RenderedMessage);
                else if (loggingEvent.Level <= Level.Warn)
                    logger.Warn(loggingEvent.RenderedMessage);
                else if (loggingEvent.Level <= Level.Error)
                    logger.Error(loggingEvent.RenderedMessage);
                else
                    logger.Fatal(loggingEvent.RenderedMessage);
            }
        }

        [Serializable]
        class Shuttle {
            public readonly MessageSink _sink = new MessageSink();
        }


        [Given(@"I have module ""(.*)""")]
        public void GivenIHaveModule(string moduleName) {
            Host.CopyExtension("Modules", moduleName, _moduleDeploymentOptions);
        }

        [Given(@"I have module ""(.*)"" with its recipes")]
        public void GivenIHaveModuleWithRecipes(string moduleName) {
            Host.CopyExtensionAndRecipes("Modules", moduleName, _moduleDeploymentOptions);
        }


        [Given(@"I have theme ""(.*)""")]
        public void GivenIHaveTheme(string themeName) {
            Host.CopyExtension("Themes", themeName, ExtensionDeploymentOptions.CompiledAssembly);
        }

        [Given(@"I have theme ""(.*)"" with its recipes")]
        public void GivenIHaveThemeWithRecipes(string themeName) {
            Host.CopyExtensionAndRecipes("Themes", themeName, ExtensionDeploymentOptions.CompiledAssembly);
        }

        [Given(@"I have core ""(.*)""")]
        public void GivenIHaveCore(string moduleName) {
            Host.CopyExtension("Core", moduleName, ExtensionDeploymentOptions.CompiledAssembly);
        }

        [Given(@"I have a clean site with")]
        public void GivenIHaveACleanSiteWith(Table table) {
            GivenIHaveACleanSiteWith("/", table);
        }


        [Given(@"I have a clean site at ""(.*)"" with")]
        public void GivenIHaveACleanSiteWith(string virtualDirectory, Table table) {
            GivenIHaveACleanSite(virtualDirectory);
            foreach (var row in table.Rows) {
                foreach (var name in row["names"].Split(',').Select(x => x.Trim())) {
                    switch (row["extension"]) {
                        case "Core":
                            GivenIHaveCore(name);
                            break;
                        case "Module":
                            GivenIHaveModule(name);
                            break;
                        case "Theme":
                            GivenIHaveTheme(name);
                            break;
                        default:
                            Assert.Fail("Unknown extension type {0}", row["extension"]);
                            break;
                    }
                }
            }
        }

        [Given(@"I am on ""(.*)""")]
        public void GivenIAmOn(string urlPath) {
            WhenIGoTo(urlPath);
        }

        [Given(@"I have the file ""(.*)"" in ""(.*)""")]
        public void GivenIHaveFile(string sourceFileName, string destination) {
            Host.CopyFile(sourceFileName, destination);
        }

        [When(@"I go to ""(.*)"" on host (.*)")]
        public void WhenIGoToPathOnHost(string urlPath, string host) {
            Host.HostName = host;
            Details = Host.SendRequest(urlPath);
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Regex.Replace(Details.ResponseText, @">\s+<", "><")));
        }

        [When(@"I go to ""(.*)""")]
        public void WhenIGoTo(string urlPath) {
            Details = Host.SendRequest(urlPath);
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Regex.Replace(Details.ResponseText, @">\s+<", "><")));
        }

        [When(@"I try to go to ""(.*)""")]
        public void WhenITryToGoTo(string urlPath) {
            WhenITryToGoTo(urlPath, 2);
        }

        public void WhenITryToGoTo(string urlPath, int retries) {
            Details = Host.SendRequest(urlPath);
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Regex.Replace(Details.ResponseText, @">\s+<", "><")));
            if (Details.StatusCode == 302 && retries > 0) {
                var newPath = LinkUrl("here");
                WhenITryToGoTo(urlPath, retries - 1);
            }
        }

        [When(@"I post to ""(.*)""")]
        public void WhenIPostTo(string urlPath) {
            Details = Host.PostRequest(urlPath);
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Regex.Replace(Details.ResponseText, @">\s+<", "><")));
        }

        private string LinkUrl(string linkText) {
            var link = _doc.DocumentNode
                            .SelectNodes("//a")
                            .SingleOrDefault(elt => elt.InnerHtml == linkText)
                       ?? _doc.DocumentNode
                            .SelectSingleNode(string.Format("//a[@title='{0}']", linkText));

            return HttpUtility.HtmlDecode(link.Attributes["href"].Value);
        }

        private string LinkUrl(string linkText, string hrefFilter) {
            var link = _doc.DocumentNode
                            .SelectNodes("//a[@href]").Where(elt =>
                                (elt.InnerHtml == linkText ||
                                    (elt.Attributes["title"] != null && elt.Attributes["title"].Value == linkText)) &&
                                 elt.Attributes["href"].Value.IndexOf(hrefFilter, StringComparison.OrdinalIgnoreCase) != -1).SingleOrDefault();

            if (link == null) {
                throw new InvalidOperationException(string.Format("Could not find an anchor with matching text '{0}' and href '{1}'. Document: {2}", linkText, hrefFilter, _doc.DocumentNode.InnerHtml));
            }
            var href = link.Attributes["href"].Value;
            return HttpUtility.HtmlDecode(href);
        }

        private string LinkUrlWithClass(string linkText, string className) {
            var link = _doc.DocumentNode
                            .SelectNodes("//a[@href]").Where(elt =>
                                (elt.InnerText == linkText ||
                                    (elt.Attributes["title"] != null && elt.Attributes["title"].Value == linkText)) &&
                                 elt.Attributes["class"].Value.IndexOf(className, StringComparison.OrdinalIgnoreCase) != -1).SingleOrDefault();

            if (link == null) {
                throw new InvalidOperationException(string.Format("Could not find an anchor with matching text '{0}' and class '{1}'. Document: {2}", linkText, className, _doc.DocumentNode.InnerHtml));
            }
            var href = link.Attributes["href"].Value;
            return HttpUtility.HtmlDecode(href);
        }

        [When(@"I follow ""([^""]*)""")]
        public void WhenIFollow(string linkText) {
            var urlPath = LinkUrl(linkText);

            WhenIGoTo(urlPath);
        }

        [When(@"I follow ""([^""]+)"" where href has ""([^""]+)""")]
        public void WhenIFollow(string linkText, string hrefFilter) {
            var urlPath = LinkUrl(linkText, hrefFilter);

            WhenIGoTo(urlPath);
        }

        [When(@"I follow ""([^""]+)"" where class name has ""([^""]+)""")]
        public void WhenIFollowClass(string linkText, string className) {
            var urlPath = LinkUrlWithClass(linkText, className);

            WhenIGoTo(urlPath);
        }

        [When(@"I post ""([^""]*)""")]
        public void WhenIPost(string linkText) {
            var urlPath = LinkUrl(linkText);

            WhenIPostTo(urlPath);
        }

        [When(@"I post ""([^""]+)"" where href has ""([^""]+)""")]
        public void WhenIPost(string linkText, string hrefFilter) {
            var urlPath = LinkUrl(linkText, hrefFilter);

            WhenIPostTo(urlPath);
        }

        [When(@"I post ""([^""]+)"" where class name has ""([^""]+)""")]
        public void WhenIPostClass(string linkText, string className) {
            var urlPath = LinkUrlWithClass(linkText, className);

            WhenIPostTo(urlPath);
        }

        [When(@"I fill in")]
        public void WhenIFillIn(Table table) {
            var inputs = _doc.DocumentNode
                .SelectNodes("(//input|//textarea|//select)") ?? Enumerable.Empty<HtmlNode>();

            foreach (var row in table.Rows) {
                var r = row;
                var input = inputs.FirstOrDefault(x => x.GetAttributeValue("name", x.GetAttributeValue("id", "")) == r["name"]);
                Assert.That(input, Is.Not.Null, "Unable to locate <input> name {0} in page html:\r\n\r\n{1}", r["name"], Details.ResponseText);
                var inputType = input.GetAttributeValue("type", "");
                switch (inputType) {
                    case "radio":
                        var radios = inputs.Where(
                            x =>
                            x.GetAttributeValue("type", "") == "radio" &&
                            x.GetAttributeValue("name", x.GetAttributeValue("id", "")) == r["name"]);
                        foreach (var radio in radios) {
                            if (radio.GetAttributeValue("value", "") == row["value"])
                                radio.Attributes.Add("checked", "checked");
                            else if (radio.Attributes.Contains("checked"))
                                radio.Attributes.Remove("checked");
                        }
                        break;
                    case "checkbox":
                        if (string.Equals(row["value"], "true", StringComparison.OrdinalIgnoreCase)) {
                            input.Attributes.Add("checked", "checked");
                        } else {
                            input.Attributes.Remove("checked");
                        }

                        var hiddenForCheckbox = inputs.Where(
                            x =>
                            x.GetAttributeValue("type", "") == "hidden" &&
                            x.GetAttributeValue("name", x.GetAttributeValue("id", "")) == r["name"]
                            ).FirstOrDefault();
                        if (hiddenForCheckbox != null)
                            hiddenForCheckbox.Attributes.Add("value", row["value"]);

                        break;
                    default:
                        if (string.Equals(input.Name, "select", StringComparison.OrdinalIgnoreCase)) {
                            var options = input.Descendants("option");
                            foreach (var option in options) {
                                if (option.GetAttributeValue("value", "") == row["value"] || (option.NextSibling.NodeType == HtmlNodeType.Text && option.NextSibling.InnerText == row["value"]))
                                    option.Attributes.Add("selected", "selected");
                                else if (option.Attributes.Contains("selected"))
                                    option.Attributes.Remove("selected");
                            }

                        } else {
                            input.Attributes.Add("value", row["value"]);
                        }
                        break;
                }
            }
        }

        [When(@"I hit ""(.*)""")]
        public void WhenIHit(string submitText) {
            var submit = _doc.DocumentNode
                .SelectSingleNode(string.Format("(//input[@type='submit'][@value='{0}']|//button[@type='submit'][text()='{0}'])", submitText));

            string urlPath = null;

            if (submit == null) {
                // could be a simple link using "unsafeurl" property

                submit = _doc.DocumentNode
                            .SelectNodes("//a")
                            .SingleOrDefault(elt => elt.InnerHtml == submitText)
                       ?? _doc.DocumentNode
                            .SelectSingleNode(string.Format("//a[@title='{0}']", submitText));

                urlPath = HttpUtility.HtmlDecode(submit.Attributes["href"].Value);
            }

            var form = Form.LocateAround(submit);

            if (urlPath == null) {
                urlPath = HttpUtility.HtmlDecode(form.Start.GetAttributeValue("action", Details.UrlPath));
            }

            var inputs = form.Children
                    .SelectMany(elt => elt.DescendantsAndSelf("input").Concat(elt.Descendants("textarea")))
                    .Where(node => !((node.GetAttributeValue("type", "") == "radio" || node.GetAttributeValue("type", "") == "checkbox") && node.GetAttributeValue("checked", "") != "checked"))
                    .GroupBy(elt => elt.GetAttributeValue("name", elt.GetAttributeValue("id", "")), elt => elt.GetAttributeValue("value", ""))
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    // add values of <select>s
                    .Concat(
                        // select all <select> elements
                        form.Children.SelectMany(elt => elt.DescendantsAndSelf("select")).Where(elt => elt.Name.Equals("select", StringComparison.OrdinalIgnoreCase))
                            // group them by their name with value that comes from first of:
                            //  (1) value of option with 'selecturlPath.Replace("127.0.0.1", "localhost")ed' attribute,
                            //  (2) value of first option (none have 'selected'),
                            //  (3) empty value (e.g. select with no options)
                            .GroupBy(
                                sel => sel.GetAttributeValue("name", sel.GetAttributeValue("id", "")),
                                sel => (sel.Descendants("option").SingleOrDefault(opt => opt.Attributes["selected"] != null) ?? sel.Descendants("option").FirstOrDefault() ?? new HtmlNode(HtmlNodeType.Element, _doc, 0)).GetOptionValue()))
                    .ToDictionary(elt => elt.Key, elt => (IEnumerable<string>)elt);

            if (submit.Attributes.Contains("name"))
                inputs.Add(submit.GetAttributeValue("name", ""), new[] { submit.GetAttributeValue("value", "yes") });

            Details = Host.SendRequest(urlPath, inputs, form.Start.GetAttributeValue("method", "GET").ToUpperInvariant());
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Details.ResponseText));
        }

        [When(@"I am redirected")]
        public void WhenIAmRedirected() {
            var urlPath = "";
            if (Details.ResponseHeaders.TryGetValue("Location", out urlPath)) {
                WhenIGoTo(urlPath);
            } else {
                Assert.Fail("Expected to be redirected but no Location header returned");
            }
        }

        [Then(@"the status should be (.*) ""(.*)""")]
        public void ThenTheStatusShouldBe(int statusCode, string statusDescription) {
            Assert.That(Details.StatusCode, Is.EqualTo(statusCode));
            Assert.That(Details.StatusDescription, Is.EqualTo(statusDescription));
        }

        [Then(@"the content type should be ""(.*)""")]
        public void ThenTheContentTypeShouldBe(string contentType) {
            Assert.That(Details.ResponseHeaders["Content-Type"], Is.StringMatching(contentType));
        }

        [Then(@"I should see ""(.*)""")]
        public void ThenIShouldSee(string text) {
            Assert.That(Details.ResponseText, Is.StringMatching(text));
        }

        [Then(@"response starts with ""(.*)""")]
        public void ThenResponseStartsWith(string text) {
            Assert.That(Details.ResponseText, Is.StringStarting(text));
        }

        [Then(@"response contains ""(.*)""")]
        public void ThenResponseContains(string text) {
            Assert.That(Details.ResponseText, Is.StringContaining(text));
        }

        [Then(@"I should not see ""(.*)""")]
        public void ThenIShouldNotSee(string text) {
            Assert.That(Details.ResponseText, Is.Not.StringContaining(text));
        }

        [Then(@"the title contains ""(.*)""")]
        public void ThenTheTitleContainsText(string text) {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should be denied access when I go to ""(.*)""")]
        public void ThenIShouldBeDeniedAccessWhenIGoTo(string urlPath) {
            WhenIGoTo(urlPath);
            WhenIAmRedirected();
            ThenIShouldSee("Access Denied");
        }

        [Then(@"response has an element called ""(.*)"" with attribute ""(.*)"" with value ""(.*)""")]
        public void ThenResponseShouldHaveFormElementWithValue(string nodeName, string attributeName, string attributeValue) {
            var nodes = _doc.DocumentNode
                .Descendants() // find all nodes with the matching name or id
                .Where(hn => hn.GetAttributeValue("name", hn.GetAttributeValue("id", "")) == nodeName);
            Assert.That(nodes, Is.Not.Null,
                "Unable to locate name {0} in page html:\r\n\r\n{1}", nodeName, Details.ResponseText);
            Assert.That(nodes.Any(), Is.True,
                "Unable to locate name {0} in page html:\r\n\r\n{1}", nodeName, Details.ResponseText);

            var attributes = nodes
                .Select(node => node.GetAttributeValue(attributeName, null))
                .Where(at => at != null);
            Assert.That(attributes, Is.Not.Null,
                "Unable to locate attribute {0} for element {1} in page html:\r\n\r\n{2}",
                    attributeName, nodeName, Details.ResponseText);
            Assert.That(attributes.Any(), Is.True,
                "Unable to locate attribute {0} for element {1} in page html:\r\n\r\n{2}",
                    attributeName, nodeName, Details.ResponseText);

            Assert.That(
                attributes.Any(at => at.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)),
                Is.True,
                "Attribute value is not what we expected ({1})", attributeValue);
            
        }

        [Then(@"response has an element called ""(.*)"" without attribute ""(.*)""")]
        public void ThenResponseShouldHaveFormElementWithoutAttribute(string nodeName, string attributeName) {
            var nodes = _doc.DocumentNode
                .Descendants() // find all nodes with the matching name or id
                .Where(hn => hn.GetAttributeValue("name", hn.GetAttributeValue("id", "")) == nodeName);
            Assert.That(nodes, Is.Not.Null,
                "Unable to locate name {0} in page html:\r\n\r\n{1}", nodeName, Details.ResponseText);
            Assert.That(nodes.Any(), Is.True,
                "Unable to locate name {0} in page html:\r\n\r\n{1}", nodeName, Details.ResponseText);

            var attributes = nodes
                .Select(node => node.GetAttributeValue(attributeName, null))
                .Where(at => at != null);
            Assert.That(attributes.Any(), Is.False, 
                "Element {0} has an attribute {1} in page html:\r\n\r\n{2}",
                nodeName, attributeName, Details.ResponseText);
            
        }

    }

    public class Form {
        public static Form LocateAround(HtmlNode cornerstone) {
            foreach (var inspect in cornerstone.AncestorsAndSelf()) {

                var form = inspect.PreviousSiblingsAndSelf().FirstOrDefault(
                    n => n.NodeType == HtmlNodeType.Element && n.Name == "form");
                if (form == null)
                    continue;

                var endForm = inspect.NextSiblingsAndSelf().FirstOrDefault(
                    n => n.NodeType == HtmlNodeType.Text && n.InnerText == "</form>");
                if (endForm == null)
                    continue;

                return new Form {
                    Start = form,
                    End = endForm,
                    Children = form.NextSibling.NextSiblingsAndSelf().TakeWhile(n => n != endForm).ToArray()
                };
            }

            return null;
        }


        public HtmlNode Start { get; set; }
        public HtmlNode End { get; set; }
        public IEnumerable<HtmlNode> Children { get; set; }
    }

    static class HtmlExtensions {
        public static IEnumerable<HtmlNode> PreviousSiblingsAndSelf(this HtmlNode node) {
            var scan = node;
            while (scan != null) {
                yield return scan;
                scan = scan.PreviousSibling;
            }
        }
        public static IEnumerable<HtmlNode> NextSiblingsAndSelf(this HtmlNode node) {
            var scan = node;
            while (scan != null) {
                yield return scan;
                scan = scan.NextSibling;
            }
        }
    }
}
