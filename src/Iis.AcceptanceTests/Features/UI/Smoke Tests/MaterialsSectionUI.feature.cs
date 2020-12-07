﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.4.0.0
//      SpecFlow Generator Version:3.4.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace AcceptanceTests.Features.UI.SmokeTests
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class MaterialsSectionUI_SmokeFeature : object, Xunit.IClassFixture<MaterialsSectionUI_SmokeFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "MaterialsSectionUI.feature"
#line hidden
        
        public MaterialsSectionUI_SmokeFeature(MaterialsSectionUI_SmokeFeature.FixtureData fixtureData, AcceptanceTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/UI/Smoke Tests", "MaterialsSectionUI - Smoke", @"    - IIS-6187 - Ensure that Materials section is opened
    - IIS-6205 - Ensure that search by using ! symbol gives 0 search results
    - IIS-6204 - Ensure that search by using * symbol gives all possible search results
    - IIS-6188 - Ensure that the material card can be opened
    - IIS-6192 - Open events tab relation in the materials card
    - IIS-6189 - Open general tab in the materials card
    - IIS-6191 - Open objects tab in the materials card
    - IIS-6190 - Open ML tab in the materials card", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 12
    #line hidden
#line 13
        testRunner.Given("I sign in with the user olya and password 123 in the Contour", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6187 - Ensure that Materials section is opened")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6187 - Ensure that Materials section is opened")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsUI")]
        public virtual void IIS_6187_EnsureThatMaterialsSectionIsOpened()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6187 - Ensure that Materials section is opened", null, tagsOfScenario, argumentsOfScenario);
#line 17
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 18
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 19
        testRunner.Then("I must see the Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 20
        testRunner.Then("I must see first user in the user list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6205 - Ensure that search by using ! symbol gives 0 search results")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6205 - Ensure that search by using ! symbol gives 0 search results")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSearchUI")]
        public virtual void IIS_6205_EnsureThatSearchByUsingSymbolGives0SearchResults()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsSearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6205 - Ensure that search by using ! symbol gives 0 search results", null, tagsOfScenario, argumentsOfScenario);
#line 23
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 24
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 25
        testRunner.And("I clicked search button in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 26
        testRunner.And("I searched ! data in the materials", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 27
        testRunner.Then("I must see zero results in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6204 - Ensure that search by using * symbol gives all possible search results" +
            "")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6204 - Ensure that search by using * symbol gives all possible search results" +
            "")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSearchUI")]
        public virtual void IIS_6204_EnsureThatSearchByUsingSymbolGivesAllPossibleSearchResults()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsSearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6204 - Ensure that search by using * symbol gives all possible search results" +
                    "", null, tagsOfScenario, argumentsOfScenario);
#line 30
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 31
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 32
        testRunner.And("I clicked search button in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 33
        testRunner.And("I got search counter value in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 34
        testRunner.And("I searched * data in the materials", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 35
        testRunner.Then("I must see that search counter values are equal in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6188 - Ensure that the material card can be opened")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6188 - Ensure that the material card can be opened")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsCardUI")]
        public virtual void IIS_6188_EnsureThatTheMaterialCardCanBeOpened()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsCardUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6188 - Ensure that the material card can be opened", null, tagsOfScenario, argumentsOfScenario);
#line 38
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 39
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 40
        testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 41
        testRunner.Then("I must see processed button in the materials card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 42
        testRunner.Then("I must see relevance drop down in the materials card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6192 - Open events tab relation in the materials card")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6192 - Open events tab relation in the materials card")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsCardEventsTabUI")]
        public virtual void IIS_6192_OpenEventsTabRelationInTheMaterialsCard()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsCardEventsTabUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6192 - Open events tab relation in the materials card", null, tagsOfScenario, argumentsOfScenario);
#line 45
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 46
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 47
        testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 48
        testRunner.And("I clicked on the events tab in the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 49
        testRunner.Then("I must see events search in the materials card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6189 - Open general tab in the materials card")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6189 - Open general tab in the materials card")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsCardGeneralTabUI")]
        public virtual void IIS_6189_OpenGeneralTabInTheMaterialsCard()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsCardGeneralTabUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6189 - Open general tab in the materials card", null, tagsOfScenario, argumentsOfScenario);
#line 52
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 53
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 54
        testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "ImportanceDropDown"});
                table1.AddRow(new string[] {
                            "RelevanceDropDown"});
                table1.AddRow(new string[] {
                            "СompletenessOfInformation"});
                table1.AddRow(new string[] {
                            "SourceCredibility"});
                table1.AddRow(new string[] {
                            "Originator"});
#line 55
        testRunner.Then("I must see these elements", ((string)(null)), table1, "Then ");
#line hidden
#line 63
        testRunner.Then("I must I must see at least one user in the originator drop down menu", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6191 - Open objects tab in the materials card")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6191 - Open objects tab in the materials card")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsCardGeneralTabUI")]
        public virtual void IIS_6191_OpenObjectsTabInTheMaterialsCard()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsCardGeneralTabUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6191 - Open objects tab in the materials card", null, tagsOfScenario, argumentsOfScenario);
#line 66
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 67
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 68
        testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 69
        testRunner.And("I clicked on the objects tab in the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 70
        testRunner.Then("I must see objects search in the materials card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6190 - Open ML tab in the materials card")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6190 - Open ML tab in the materials card")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsCardGeneralTabUI")]
        public virtual void IIS_6190_OpenMLTabInTheMaterialsCard()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "MaterialsCardGeneralTabUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6190 - Open ML tab in the materials card", null, tagsOfScenario, argumentsOfScenario);
#line 73
    this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 12
    this.FeatureBackground();
#line hidden
#line 74
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 75
        testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 76
        testRunner.And("I clicked on the ML tab in the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 77
        testRunner.Then("I must see Show button in the ML tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.4.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                MaterialsSectionUI_SmokeFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                MaterialsSectionUI_SmokeFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion