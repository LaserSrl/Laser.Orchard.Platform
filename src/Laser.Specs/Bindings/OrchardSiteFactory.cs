using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Laser.Specs.Hosting.Orchard.Web;
using TechTalk.SpecFlow;

namespace Laser.Specs.Bindings {
    [Binding]
    public class OrchardSiteFactory : BindingBase {
        [Given(@"I have installed Orchard")]
        public void GivenIHaveInstalledOrchard() {
            GivenIHaveInstalledOrchard("/");
        }

        [Given(@"I have installed Orchard at ""(.*)\""")]
        public void GivenIHaveInstalledOrchard(string virtualDirectory) {
            var webApp = Binding<WebAppHosting>();

            // this is the list of module which will be copied over into the temporary Orchard folder
            var moduleNames = "Lucene, Markdown, Orchard.Alias, Orchard.AntiSpam, Orchard.ArchiveLater, Orchard.Autoroute, Orchard.Azure, Orchard.Blogs, Orchard.Caching, Orchard.CodeGeneration, Orchard.Comments, Orchard.ContentPermissions, Orchard.ContentPicker, Orchard.ContentTypes, Orchard.DesignerTools, Orchard.Email, Orchard.Fields, Orchard.Forms, Orchard.ImageEditor, Orchard.ImportExport, Orchard.Indexing, Orchard.JobsQueue, Orchard.Resources, Orchard.Layouts, Orchard.Lists, Orchard.Localization, Orchard.MediaLibrary, Orchard.MediaProcessing, Orchard.Migrations, Orchard.Modules, Orchard.MultiTenancy, Orchard.OutputCache, Orchard.Packaging, Orchard.Pages, Orchard.Projections, Orchard.PublishLater, Orchard.Recipes, Orchard.Roles, Orchard.Scripting, Orchard.Scripting.CSharp, Orchard.Scripting.Dlr, Orchard.Search, Orchard.SecureSocketsLayer, Orchard.Setup, Orchard.Tags, Orchard.Taxonomies, Orchard.Templates, Orchard.Themes, Orchard.Tokens, Orchard.Users, Orchard.Warmup, Orchard.Widgets, Orchard.Workflows, Orchard.Conditions, SysCache, TinyMce, Upgrade";
            var themeNames = "SafeMode, TheAdmin, TheThemeMachine";

            webApp.GivenIHaveACleanSiteWith(
                virtualDirectory,
                TableData(
                new { extension = "Module", names = moduleNames },
                new { extension = "Core", names = "Common, Containers, Contents, Dashboard, Feeds, Navigation, Scheduling, Settings, Shapes, Title, XmlRpc" },
                new { extension = "Theme", names = themeNames }));

            webApp.WhenIGoTo("Setup");

            webApp.WhenIFillIn(TableData(
                new { name = "SiteName", value = "My Site" },
                new { name = "AdminPassword", value = "6655321" },
                new { name = "ConfirmPassword", value = "6655321" }));

            webApp.WhenIHit("Finish Setup");
        }

        [Given(@"I have installed Krake")]
        public void GivenIHaveInstalledKrake() {
            GivenIHaveInstalledKrake("/");
        }

        [Given(@"I have installed Krake at ""(.*)\""")]
        public void GivenIHaveInstalledKrake(string virtualDirectory) {
            var webApp = Binding<WebAppHosting>();

            // this is the list of module which will be copied over into the temporary Orchard folder
            var moduleNames = "Lucene, Markdown, Orchard.Alias, Orchard.AntiSpam, Orchard.ArchiveLater, Orchard.Autoroute, Orchard.Azure, Orchard.Blogs, Orchard.Caching, Orchard.CodeGeneration, Orchard.Comments, Orchard.ContentPermissions, Orchard.ContentPicker, Orchard.ContentTypes, Orchard.DesignerTools, Orchard.Email, Orchard.Fields, Orchard.Forms, Orchard.ImageEditor, Orchard.ImportExport, Orchard.Indexing, Orchard.JobsQueue, Orchard.Resources, Orchard.Layouts, Orchard.Lists, Orchard.Localization, Orchard.MediaLibrary, Orchard.MediaProcessing, Orchard.Migrations, Orchard.Modules, Orchard.MultiTenancy, Orchard.OutputCache, Orchard.Packaging, Orchard.Pages, Orchard.Projections, Orchard.PublishLater, Orchard.Recipes, Orchard.Roles, Orchard.Scripting, Orchard.Scripting.CSharp, Orchard.Scripting.Dlr, Orchard.Search, Orchard.SecureSocketsLayer, Orchard.Setup, Orchard.Tags, Orchard.Taxonomies, Orchard.Templates, Orchard.Themes, Orchard.Tokens, Orchard.Users, Orchard.Warmup, Orchard.Widgets, Orchard.Workflows, Orchard.Conditions, SysCache, TinyMce, Upgrade";
            moduleNames = string.Join(", ", moduleNames, string.Join(", ", LaserBindings.LaserModuleNames));
            var themeNames = "SafeMode, TheAdmin, TheThemeMachine";
            themeNames = string.Join(", ", themeNames, string.Join(", ", LaserBindings.LaserThemeNames));

            webApp.GivenIHaveACleanSiteWith(
                virtualDirectory,
                TableData(
                new { extension = "Module", names = moduleNames },
                new { extension = "Core", names = "Common, Containers, Contents, Dashboard, Feeds, Navigation, Scheduling, Settings, Shapes, Title, XmlRpc" },
                new { extension = "Theme", names = themeNames }));

            webApp.WhenIGoTo("Setup");

            webApp.WhenIFillIn(TableData(
                new { name = "SiteName", value = "My Site" },
                new { name = "AdminPassword", value = "6655321" },
                new { name = "ConfirmPassword", value = "6655321" }));

            webApp.WhenIHit("Finish Setup");
        }

        [Given(@"I have installed ""(.*)\""")]
        public void GivenIHaveInstalled(string name) {
            GivenIHaveInstalledAndNotEnabled(name);

            GivenIHaveEnabled(name);
        }

        [Given(@"I have installed ""(.*)\"" with its recipes")]
        public void GivenIHaveInstalledWithRecipes(string name) {
            GivenIHaveInstalledAndNotEnabledWithRecipes(name);

            GivenIHaveEnabled(name);
        }

        [Given(@"I have installed and not enabled ""(.*)\"" with its recipes")]
        public void GivenIHaveInstalledAndNotEnabledWithRecipes(string name) {
            var webApp = Binding<WebAppHosting>();
            webApp.GivenIHaveModuleWithRecipes(name);
            webApp.Host.Execute(MvcApplication.ReloadExtensions);
        }

        [Given(@"I have installed and not enabled ""(.*)\""")]
        public void GivenIHaveInstalledAndNotEnabled(string name) {
            var webApp = Binding<WebAppHosting>();
            webApp.GivenIHaveModule(name);
            webApp.Host.Execute(MvcApplication.ReloadExtensions);
        }

        [Given(@"I have enabled ""(.*)\""")]
        public void GivenIHaveEnabled(string name) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var descriptorManager = environment.Resolve<IShellDescriptorManager>();
                    var descriptor = descriptorManager.GetShellDescriptor();
                    descriptorManager.UpdateShellDescriptor(
                        descriptor.SerialNumber,
                        descriptor.Features.Concat(new[] { new ShellFeature { Name = name } }),
                        descriptor.Parameters);
                }

                // this is needed to force the tenant to restart when a new feature is enabled,
                // as DefaultOrchardHost maintains this list in a thread context otherwise
                // and looses the information
                MvcApplication.RestartTenant("Default");
            });
            //webApp.Host.Execute(MvcApplication.ReloadExtensions);
        }

        [Given(@"I restart tenant")]
        public void GivenIRestartTenant() {
            GivenIRestartTenant("Default");
        }

        [Given(@"I restart tenant named ""(.*)\""")]
        public void GivenIRestartTenant(string shellName = "Default") {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(()=> {
                var name = shellName;
                MvcApplication.RestartTenant(name);
            });
        }

        [Given(@"I have a containable content type ""(.*)\""")]
        public void GivenIHaveAContainableContentType(string name) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    var cdm = environment.Resolve<IContentDefinitionManager>();

                    var contentTypeDefinition = new ContentTypeDefinition(name, name);
                    cdm.StoreTypeDefinition(contentTypeDefinition);
                    cdm.AlterTypeDefinition(name, cfg => cfg.WithPart("CommonPart").WithPart("BodyPart").WithPart("TitlePart").WithPart("ContainablePart").Creatable().Draftable());

                    cdm.AlterTypeDefinition(name,
                        cfg => cfg.WithPart("AutoroutePart",
                            builder => builder
                                .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                                .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                                .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-list'}]")
                                .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                        ));

                }
            });
        }

        [Given(@"I have tenant ""(.*)\"" on ""(.*)\"" as ""(.*)\""")]
        public void GivenIHaveTenantOnSiteAsName(string shellName, string hostName, string siteName) {
            var webApp = Binding<WebAppHosting>();
            webApp.Host.Execute(() => {
                var shellSettings = new ShellSettings {
                    Name = shellName,
                    RequestUrlHost = hostName,
                    State = TenantState.Uninitialized,
                };
                using (var environment = MvcApplication.CreateStandaloneEnvironment("Default")) {
                    environment.Resolve<IShellSettingsManager>().SaveSettings(shellSettings);
                }

                MvcApplication.RestartTenant(shellName);
            });

            webApp.WhenIGoToPathOnHost("Setup", hostName);

            webApp.WhenIFillIn(TableData(
                new { name = "SiteName", value = siteName },
                new { name = "AdminPassword", value = "6655321" }));

            webApp.WhenIHit("Finish Setup");
        }
    }
}
