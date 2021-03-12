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
    public partial class ObjectsOfStudySection_SmokeFeature : object, Xunit.IClassFixture<ObjectsOfStudySection_SmokeFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ObjectsOfStudySection.feature"
#line hidden
        
        public ObjectsOfStudySection_SmokeFeature(ObjectsOfStudySection_SmokeFeature.FixtureData fixtureData, AcceptanceTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/UI/Smoke Tests", "ObjectsOfStudySection - smoke", @"    - IIS-6209 - Ensure that search by using * symbol gives all possible search results
    - IIS-6210 - Ensure that search by using ! symbol gives 0 search results
    - IIS-6207 - Open a small object of study card
    - IIS-6208 - Open a big object of study card", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 8
    #line hidden
#line 9
        testRunner.Given("I sign in with the user olya and password 123 in the Contour", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6209 - Ensure that search by using * symbol gives all possible search results" +
            "")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6209 - Ensure that search by using * symbol gives all possible search results" +
            "")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySearchUI")]
        public virtual void IIS_6209_EnsureThatSearchByUsingSymbolGivesAllPossibleSearchResults()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "ObjectOfStudySearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6209 - Ensure that search by using * symbol gives all possible search results" +
                    "", null, tagsOfScenario, argumentsOfScenario);
#line 12
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
#line 8
    this.FeatureBackground();
#line hidden
#line 13
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 14
        testRunner.And("I got search counter value in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 15
        testRunner.And("I searched * data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 16
        testRunner.Then("I must see that search counter values are equal in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6210 - Ensure that search by using ! symbol gives 0 search results")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6210 - Ensure that search by using ! symbol gives 0 search results")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySearchUI")]
        public virtual void IIS_6210_EnsureThatSearchByUsingSymbolGives0SearchResults()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "ObjectOfStudySearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6210 - Ensure that search by using ! symbol gives 0 search results", null, tagsOfScenario, argumentsOfScenario);
#line 19
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
#line 8
    this.FeatureBackground();
#line hidden
#line 20
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 21
        testRunner.And("I searched ! data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 22
        testRunner.Then("I must see zero search results in the Object of study page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6207 - Open a small object of study card")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6207 - Open a small object of study card")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySmallCardUI")]
        public virtual void IIS_6207_OpenASmallObjectOfStudyCard()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "sanity",
                    "UI",
                    "ObjectOfStudySmallCardUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6207 - Open a small object of study card", null, tagsOfScenario, argumentsOfScenario);
#line 25
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
#line 8
    this.FeatureBackground();
#line hidden
#line 26
        testRunner.When("I clicked on first object of study", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 27
        testRunner.Then("I must see the object of study small card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 28
        testRunner.Then("I must see the title Тестовий Центр ППО in the small card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6208 - Open a big object of study card")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - smoke")]
        [Xunit.TraitAttribute("Description", "IIS-6208 - Open a big object of study card")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySmallCardUI")]
        public virtual void IIS_6208_OpenABigObjectOfStudyCard()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "sanity",
                    "UI",
                    "ObjectOfStudySmallCardUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6208 - Open a big object of study card", null, tagsOfScenario, argumentsOfScenario);
#line 31
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
#line 8
    this.FeatureBackground();
#line hidden
#line 32
        testRunner.When("I clicked on first object of study", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 33
        testRunner.And("I clicked on enlarge small card button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                            "BigCardProfileTab"});
                table5.AddRow(new string[] {
                            "BigCardMaterialsTab"});
                table5.AddRow(new string[] {
                            "BigCardEventsTab"});
                table5.AddRow(new string[] {
                            "BigCardChangeHistoryTab"});
                table5.AddRow(new string[] {
                            "BigCardRelationsTab"});
#line 34
        testRunner.Then("I must see these tabs in the big object of study card", ((string)(null)), table5, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                            "BigCardAffiliation"});
                table6.AddRow(new string[] {
                            "BigCardImportance"});
#line 42
        testRunner.Then("I must see the specific text blocks in big object of study card", ((string)(null)), table6, "Then ");
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
                ObjectsOfStudySection_SmokeFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                ObjectsOfStudySection_SmokeFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
