﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Laser.Specs
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("GDPR")]
    public partial class GDPRFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Laser.Orchard.GDPR.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "GDPR", "In order to avoid silly mistakes\nAs a math idiot\nI want to be told the sum of two" +
                    " numbers", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("The GDPR Module should be there with all its features")]
        [NUnit.Framework.CategoryAttribute("mytag")]
        public virtual void TheGDPRModuleShouldBeThereWithAllItsFeatures()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("The GDPR Module should be there with all its features", new string[] {
                        "mytag"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 10
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
 testRunner.When("I go to \"Admin/Modules/Features\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
 testRunner.Then("I should see \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 14
  testRunner.And("I should see \"Laser.Orchard.GDPR.Workflows\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
  testRunner.And("I should see \"Laser.Orchard.GDPR.ContentPickerFieldExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
  testRunner.And("I should see \"Laser.Orchard.GDPR.MediaExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 17
  testRunner.And("I should see \"Laser.Orchard.GDPR.Scheduling\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("The extension feature for contacts is there")]
        public virtual void TheExtensionFeatureForContactsIsThere()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("The extension feature for contacts is there", ((string[])(null)));
#line 26
this.ScenarioSetup(scenarioInfo);
#line 27
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 30
  testRunner.And("I have installed and not enabled \"Laser.Orchard.Queries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
  testRunner.And("I have installed and not enabled \"Laser.Orchard.ShortLinks\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
  testRunner.And("I have installed and not enabled \"Laser.Orchard.StartupConfig\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 33
  testRunner.And("I have installed and not enabled \"Laser.Orchard.ZoneAlternates\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 34
  testRunner.And("I have installed and not enabled \"Laser.Orchard.jQueryPlugins\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 35
  testRunner.And("I have installed \"Laser.Orchard.CommunicationGateway\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.When("I try to go to \"Admin/Modules/Features\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 39
 testRunner.Then("I should see \"Laser.Orchard.GDPR.ContactExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("The extension features for mobile are there")]
        public virtual void TheExtensionFeaturesForMobileAreThere()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("The extension features for mobile are there", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line 42
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 44
  testRunner.And("I have installed and not enabled \"Laser.Orchard.StartupConfig\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 45
  testRunner.And("I have installed and not enabled \"Laser.Orchard.jQueryPlugins\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
  testRunner.And("I have installed and not enabled \"Laser.Orchard.Queries\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
  testRunner.And("I have installed \"Laser.Orchard.Mobile\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 50
 testRunner.When("I try to go to \"Admin/Modules/Features\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 51
 testRunner.Then("I should see \"Laser.Orchard.GDPR.MobileExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 52
  testRunner.And("I should see \"Laser.Orchard.GDPR.PushGatewayExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 53
  testRunner.And("I should see \"Laser.Orchard.GDPR.SmsGatewayExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
  testRunner.And("I should see \"Laser.Orchard.GDPR.SmsExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("The extension feature for OpenAuth is there")]
        public virtual void TheExtensionFeatureForOpenAuthIsThere()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("The extension feature for OpenAuth is there", ((string[])(null)));
#line 56
this.ScenarioSetup(scenarioInfo);
#line 57
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 59
  testRunner.And("I have installed and not enabled \"Laser.Orchard.StartupConfig\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 60
  testRunner.And("I have installed \"Laser.Orchard.OpenAuthentication\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 62
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 63
 testRunner.When("I try to go to \"Admin/Modules/Features\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 64
 testRunner.Then("I should see \"Laser.Orchard.GDPR.OpenAuthExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("GDPRPart can be attached to a content type")]
        public virtual void GDPRPartCanBeAttachedToAContentType()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("GDPRPart can be attached to a content type", ((string[])(null)));
#line 67
this.ScenarioSetup(scenarioInfo);
#line 68
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 70
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 72
 testRunner.When("I go to \"Admin/ContentTypes/ListParts\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 73
 testRunner.Then("the status should be 200 \"OK\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 75
  testRunner.And("I should see \"G D P R\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
 testRunner.When("I go to \"Admin/ContentTypes/Create\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table1.AddRow(new string[] {
                        "DisplayName",
                        "MyGDPRItem"});
            table1.AddRow(new string[] {
                        "Name",
                        "MyGDPRItem"});
#line 78
        testRunner.And("I fill in", ((string)(null)), table1, "And ");
#line 82
        testRunner.And("I hit \"Create\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 84
  testRunner.And("I go to \"Admin/ContentTypes/AddPartsTo/MyGDPRItem\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 85
 testRunner.Then("I should see \"G D P R\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 86
  testRunner.And("I should see \"value=\"GDPRPart\"\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("The GDPR permissions are there and can be configured")]
        public virtual void TheGDPRPermissionsAreThereAndCanBeConfigured()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("The GDPR permissions are there and can be configured", ((string[])(null)));
#line 88
this.ScenarioSetup(scenarioInfo);
#line 89
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 91
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 92
 testRunner.When("I go to \"Admin/Roles\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 93
 testRunner.Then("I should see \"Administrator\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 95
 testRunner.When("I go to \"Admin/Roles/Edit/1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 97
 testRunner.Then("I should see \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 99
  testRunner.And("I should see \"Effective.ManageAnonymization\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 100
  testRunner.And("I should see \"Effective.ManageErasure\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 101
  testRunner.And("I should see \"Effective.ManageItemProtection\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Admin can see and edit type settings for GDPRPart")]
        public virtual void AdminCanSeeAndEditTypeSettingsForGDPRPart()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Admin can see and edit type settings for GDPRPart", ((string[])(null)));
#line 103
this.ScenarioSetup(scenarioInfo);
#line 104
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 106
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 108
  testRunner.And("I have a type \"MyGDPRItem\" with part \"GDPRPart\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 109
  testRunner.And("I have a type \"MyGDPRItem\" with part \"TitlePart\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 110
  testRunner.And("I have a type \"MyGDPRItem\" with a \"TextField\" field called \"MyTextField\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 112
 testRunner.When("I go to \"Admin/ContentTypes/Edit/MyGDPRItem\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 114
 testRunner.Then("response contains \"Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemTyp" +
                    "e\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 115
  testRunner.And("response contains \"Parts[GDPRPart].GDPRPartTypeSettingsViewModel.DeleteItemsAfter" +
                    "Erasure\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 117
  testRunner.And("response contains \"Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldAnonymize" +
                    "\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 118
  testRunner.And("response contains \"Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldErase\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 120
  testRunner.And("response contains \"Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldAnony" +
                    "mize\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 121
  testRunner.And("response contains \"Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldErase" +
                    "\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 123
 testRunner.When("I go to \"Admin/ContentTypes/Edit/MyGDPRItem\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "name",
                        "value"});
            table2.AddRow(new string[] {
                        "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemType",
                        "True"});
            table2.AddRow(new string[] {
                        "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.DeleteItemsAfterErasure",
                        "True"});
            table2.AddRow(new string[] {
                        "Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldAnonymize",
                        "True"});
            table2.AddRow(new string[] {
                        "Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldErase",
                        "True"});
#line 126
  testRunner.And("I fill in", ((string)(null)), table2, "And ");
#line 132
  testRunner.And("I hit \"Save\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 134
  testRunner.And("I go to \"Admin/ContentTypes/Edit/MyGDPRItem\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 136
 testRunner.Then("response has an element called \"Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsP" +
                    "rofileItemType\" with attribute \"checked\" with value \"checked\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 137
  testRunner.And("response has an element called \"Parts[GDPRPart].GDPRPartTypeSettingsViewModel.Del" +
                    "eteItemsAfterErasure\" with attribute \"checked\" with value \"checked\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 138
  testRunner.And("response has an element called \"Parts[TitlePart].GDPRPartPartSettingsViewModel.Sh" +
                    "ouldAnonymize\" with attribute \"checked\" with value \"checked\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 139
  testRunner.And("response has an element called \"Fields[MyTextField].GDPRPartFieldSettingsViewMode" +
                    "l.ShouldErase\" with attribute \"checked\" with value \"checked\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 141
  testRunner.And("response has an element called \"Parts[TitlePart].GDPRPartPartSettingsViewModel.Sh" +
                    "ouldErase\" without attribute \"checked\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 142
  testRunner.And("response has an element called \"Fields[MyTextField].GDPRPartFieldSettingsViewMode" +
                    "l.ShouldAnonymize\" without attribute \"checked\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Admin can see the settings for ContentPickerFields")]
        public virtual void AdminCanSeeTheSettingsForContentPickerFields()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Admin can see the settings for ContentPickerFields", ((string[])(null)));
#line 144
this.ScenarioSetup(scenarioInfo);
#line 145
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 147
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 148
  testRunner.And("I have enabled \"Laser.Orchard.GDPR.ContentPickerFieldExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 150
  testRunner.And("I have a type \"MyGDPRItem\" with part \"GDPRPart\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 151
  testRunner.And("I have a type \"MyGDPRItem\" with part \"TitlePart\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 152
  testRunner.And("I have a type \"MyGDPRItem\" with a \"ContentPickerField\" field called \"MyCPF\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 154
 testRunner.When("I go to \"Admin/ContentTypes/Edit/MyGDPRItem\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 156
 testRunner.Then("response contains \"Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemTyp" +
                    "e\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 158
  testRunner.And("response contains \"Fields[MyCPF].ContentPickerFieldGDPRPartFieldSettingsViewModel" +
                    ".AttemptToAnonymizeItems\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Admin can see the settings for MediaLibraryPickerFields")]
        public virtual void AdminCanSeeTheSettingsForMediaLibraryPickerFields()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Admin can see the settings for MediaLibraryPickerFields", ((string[])(null)));
#line 160
this.ScenarioSetup(scenarioInfo);
#line 161
 testRunner.Given("I have installed Orchard", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 163
  testRunner.And("I have installed \"Laser.Orchard.GDPR\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 164
  testRunner.And("I have enabled \"Laser.Orchard.GDPR.MediaExtension\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 166
  testRunner.And("I have a type \"MyGDPRItem\" with part \"GDPRPart\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 167
  testRunner.And("I have a type \"MyGDPRItem\" with part \"TitlePart\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 168
  testRunner.And("I have a type \"MyGDPRItem\" with a \"MediaLibraryPickerField\" field called \"MyMLPF\"" +
                    "", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 170
 testRunner.When("I go to \"Admin/ContentTypes/Edit/MyGDPRItem\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 172
 testRunner.Then("response contains \"Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemTyp" +
                    "e\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 174
  testRunner.And("response contains \"Fields[MyMLPF].MediaLibraryPickerFieldGDPRPartFieldSettingsVie" +
                    "wModel.AttemptToAnonymizeItems\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
