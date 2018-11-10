using Laser.Specs.Hosting;
using Laser.Specs.Hosting.Orchard.Web;
using Orchard.Commands;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Features;
using Orchard.Parameters;
using Orchard.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Laser.Specs.Bindings {
    [Binding]
    public class LaserBindings : BindingBase {

        const string LaserRecipeCommand = @"recipes execute Laser.Orchard.StartupConfig Laser";
        const string RecipeResultCommand = @"recipes result {0}"; // needs the recipe ID

        //recipes execute <extensionId> <recipe>
        [Given("I have run Laser StartupConfig recipe")]
        public void GivenIHaveRunLaserRecipe() {
            // The Orchard.Recipe module should be enabled by default
            // we still have to install and enable Laser.Orchard.StartupConfig
            //Given(@"I have installed "Laser.Orchard.StartupConfig"");
            var details = new RequestDetails();
            Binding<WebAppHosting>().Host.Execute(() => {
                details = SendCommand(LaserRecipeCommand);

                // check recipe execution results
                if (details.ResponseText.IndexOf("Recipe successfully scheduled") >= 0) {
                    // get the execution ID
                    var ID = details.ResponseText
                        .Split('.')[0] // first sentence
                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Last(); // last word
                    if (!string.IsNullOrWhiteSpace(ID)) {
                        details = SendCommand(string.Format(RecipeResultCommand, ID));
                    }
                }
            });

            Binding<WebAppHosting>().Details = details;
        }

        private RequestDetails SendCommand(string command) {
            var details = new RequestDetails();
            var args = new CommandLineParser().Parse(command);
            var parameters = new CommandParametersParser().Parse(args);
            var agent = new CommandHostAgent();
            var input = new StringReader("");
            var output = new StringWriter();
            details.StatusCode = (int)agent.RunSingleCommand(
                input,
                output,
                "Default",
                parameters.Arguments.ToArray(),
                parameters.Switches.ToDictionary(kv => kv.Key, kv => kv.Value));
            details.StatusDescription = details.StatusCode.ToString();
            details.ResponseText = output.ToString();

            return details;
        }

        public static IEnumerable<string> LaserModuleNames {
            // TODO: write code to make this dynamic, by reading the .sln file
            get {
                return new string[] {
                    "Contrib.Profile",
                    "Contrib.Reviews",
                    "Contrib.Voting",
                    "Contrib.Widgets",
                    //"Four2n.MiniProfiler",
                    "itWORKS.ExtendedRegistration",
                    "Laser.Orchard.Accessibility",
                    "Laser.Orchard.AdminToolbarExtensions",
                    "Laser.Orchard.AdvancedSearch",
                    "Laser.Orchard.AppDirect",
                    "Laser.Orchard.BikeSharing",
                    "Laser.Orchard.Braintree",
                    "Laser.Orchard.ButtonToWorkflows",
                    "Laser.Orchard.Cache",
                    "Laser.Orchard.Caligoo",
                    "Laser.Orchard.ChartaWS",
                    "Laser.Orchard.CommunicationGateway",
                    "Laser.Orchard.ContactForm",
                    "Laser.Orchard.ContentExtension",
                    "Laser.Orchard.Cookies",
                    "Laser.Orchard.CulturePicker",
                    "Laser.Orchard.DataProtection",
                    "Laser.Orchard.DevTools",
                    "Laser.Orchard.DynamicNavigation",
                    "Laser.Orchard.Events",
                    "Laser.Orchard.ExternalContent",
                    "Laser.Orchard.Facebook",
                    "Laser.Orchard.FAQ",
                    "Laser.Orchard.Fidelity",
                    "Laser.Orchard.FidelityGateway",
                    "Laser.Orchard.FidelityLoyalzoo",
                    "Laser.Orchard.FidelitySimsol",
                    "Laser.Orchard.Generator",
                    "Laser.Orchard.GoogleAnalytics",
                    "Laser.Orchard.HID",
                    "Laser.Orchard.HiddenFields",
                    "Laser.Orchard.Highlights",
                    "Laser.Orchard.InsertStuff",
                    "Laser.Orchard.jQueryPlugins",
                    "Laser.Orchard.MailCommunication",
                    "Laser.Orchard.Maps",
                    "Laser.Orchard.Mobile",
                    "Laser.Orchard.MultiStepAuthentication",
                    "Laser.Orchard.NewsLetters",
                    "Laser.Orchard.NwazetIntegration",
                    "Laser.Orchard.OpenAuthentication",
                    "Laser.Orchard.PaymentCartaSi",
                    "Laser.Orchard.PaymentGateway",
                    "Laser.Orchard.PaymentGestPay",
                    "Laser.Orchard.Pdf",
                    "Laser.Orchard.Policy",
                    "Laser.Orchard.Queries",
                    "Laser.Orchard.Questionnaires",
                    "Laser.Orchard.RazorScripting",
                    "Laser.Orchard.Reporting",
                    "Laser.Orchard.SEO",
                    "Laser.Orchard.ShareLink",
                    "Laser.Orchard.Sharing",
                    "Laser.Orchard.ShortLinks",
                    "Laser.Orchard.StartupConfig",
                    "Laser.Orchard.TaskScheduler",
                    "Laser.Orchard.TemplateManagement",
                    "Laser.Orchard.Translator",
                    "Laser.Orchard.Twitter",
                    "Laser.Orchard.UserProfiler",
                    "Laser.Orchard.UserReactions",
                    "Laser.Orchard.UsersExtensions",
                    "Laser.Orchard.Vimeo",
                    "Laser.Orchard.WebServices",
                    "Laser.Orchard.ZoneAlternates",
                    "Nwazet.Commerce",
                    "Orchard.Captcha",
                    "Proligence.QrCodes"
                };
            }
        }

        public static IEnumerable<string> LaserThemeNames {
            // TODO: write code to make this dynamic, by reading the .sln file
            get {
                return new string[] {
                    "AnfiteatroMorenico",
                    "AutoMotoEpoca",
                    "BaroLegnami",
                    "inarCassa",
                    "IvreaMontalto",
                    "KrakeDefaultTheme",
                    "Laser.Bootstrap",
                    "Laser.ECommerce",
                    "Lasergroup",
                    "OctoTelematics",
                    "Proton",
                    "ResponsiveBaseTheme",
                    "zAgroPlus",
                    "KrakeAdmin",
                    "TheLaserAdmin"
                };
            }
        }

        [Given(@"I have all Laser modules")]
        public void GivenIHaveLaserModules() {

            var webApp = Binding<WebAppHosting>();
            foreach (var name in LaserModuleNames) {
                webApp.GivenIHaveModuleWithRecipes(name);
            }
            webApp.Host.Execute(MvcApplication.ReloadExtensions);
        }

        [Given(@"I have all Laser themes")]
        public void GivenIHaveLaserThemes() {
            var webApp = Binding<WebAppHosting>();
            foreach (var name in LaserThemeNames) {
                webApp.GivenIHaveThemeWithRecipes(name);
            }
            webApp.Host.Execute(MvcApplication.ReloadExtensions);
        }

        [Given(@"I have a type ""(.*)"" with part ""(.*)""")]
        public void GivenIHaveTypeWithPart(string typeName, string partName) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {

                    var cdm = environment.Resolve<IContentDefinitionManager>();

                    var contentTypeDefinition = cdm.GetTypeDefinition(typeName);
                    if (contentTypeDefinition == null) {
                        contentTypeDefinition = new ContentTypeDefinition(typeName, typeName);
                        cdm.StoreTypeDefinition(contentTypeDefinition);
                        // We should also create the definition for the part with the same name as the type
                        cdm.AlterPartDefinition(typeName, builder => builder.Attachable());
                    }

                    // We have to copy the variable like this, because the runtime is unable to 
                    // correctly serialize nested actions with parameters from different scopes
                    var tName = typeName;
                    var pName = partName;
                    cdm.AlterTypeDefinition(typeName, cfg =>
                        cfg.WithPart(tName).WithPart(pName));
                }
            });
        }

        [Given(@"I have a type ""(.*)"" with a ""(.*)"" field called ""(.*)""")]
        public void GivenIHaveATypeWithField(string typeName, string fieldType, string fieldName) {
            // the part for a type is a part with its own name
            GivenIHaveATypeWithFieldInPart(typeName, fieldType, fieldName, typeName);
        }

        [Given(@"I have a type ""(.*)"" with a ""(.*)"" field called ""(.*)"" in a ""(.*)"" part")]
        public void GivenIHaveATypeWithFieldInPart(
            string typeName, string fieldType, string fieldName, string partName) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var cdm = environment.Resolve<IContentDefinitionManager>();

                    var contentTypeDefinition = cdm.GetTypeDefinition(typeName);
                    if (contentTypeDefinition == null) {
                        contentTypeDefinition = new ContentTypeDefinition(typeName, typeName);
                        cdm.StoreTypeDefinition(contentTypeDefinition);
                        // We should also create the definition for the part with the same name as the type
                        cdm.AlterPartDefinition(typeName, builder => builder.Attachable());
                    }

                    var tName = typeName;
                    var pName = partName;
                    var fName = fieldName.ToSafeName();
                    var fType = fieldType;
                    cdm.AlterTypeDefinition(typeName, cfg => cfg
                        .WithPart(tName) // the part named like the type
                        .WithPart(pName)); // the part we'll attach the field to

                    cdm.AlterPartDefinition(pName, partBuilder => partBuilder
                        .WithField(fName, fieldBuilder => fieldBuilder
                            .OfType(fType)
                            .WithDisplayName(fName)));
                    
                }
            });
        }

        
    }
}
