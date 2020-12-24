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
namespace AcceptanceTests.Features.UI.SanityTests
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class ObjectsOfStudySection_SanityFeature : object, Xunit.IClassFixture<ObjectsOfStudySection_SanityFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ObjectsOfStudy.feature"
#line hidden
        
        public ObjectsOfStudySection_SanityFeature(ObjectsOfStudySection_SanityFeature.FixtureData fixtureData, AcceptanceTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/UI/Sanity Tests", "ObjectsOfStudySection - sanity", @"    - IIS-6119 - Possibility to switch between hierarchy objects in the OOS section
    - IIS-6211 - Search results must contain a specific result
    - IIS-6370 - View and interact with data in profile in the objects section
    - IIS-5885 - Create a military organization", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6119 - Possibility to switch between hierarchy objects in the OOS section")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - sanity")]
        [Xunit.TraitAttribute("Description", "IIS-6119 - Possibility to switch between hierarchy objects in the OOS section")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySectionUI")]
        public virtual void IIS_6119_PossibilityToSwitchBetweenHierarchyObjectsInTheOOSSection()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "ObjectOfStudySectionUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6119 - Possibility to switch between hierarchy objects in the OOS section", null, tagsOfScenario, argumentsOfScenario);
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
        testRunner.When("I clicked on Hierarchy tab in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 14
        testRunner.And("I double clicked on the Силові структури card in the hierarchy", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "ФСБ РФ"});
                table1.AddRow(new string[] {
                            "СЗР РФ"});
                table1.AddRow(new string[] {
                            "ФСВНГ РФ"});
                table1.AddRow(new string[] {
                            "ЗС РФ"});
#line 15
        testRunner.Then("I must see these cards in hierarchy", ((string)(null)), table1, "Then ");
#line hidden
#line 22
        testRunner.When("I double clicked on the ЗС РФ expand button in the hierarchy", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                            "ЗВО"});
                table2.AddRow(new string[] {
                            "ГШ ЗС РФ"});
                table2.AddRow(new string[] {
                            "Центральні органи військового управління"});
                table2.AddRow(new string[] {
                            "ОСК Північ"});
#line 23
        testRunner.Then("I must see these cards in hierarchy", ((string)(null)), table2, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6211 - Search results must contain third brigade Berkut")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - sanity")]
        [Xunit.TraitAttribute("Description", "IIS-6211 - Search results must contain third brigade Berkut")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySectionUI")]
        public virtual void IIS_6211_SearchResultsMustContainThirdBrigadeBerkut()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "ObjectOfStudySectionUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6211 - Search results must contain third brigade Berkut", null, tagsOfScenario, argumentsOfScenario);
#line 32
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
#line 33
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 34
        testRunner.And("I searched 3 омсбр data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
                TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                            "3 окрема мотострілецька бригада \"Беркут\""});
#line 35
        testRunner.Then("I must see the specified result", ((string)(null)), table3, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6370 - View and interact with data in profile in the objects section")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - sanity")]
        [Xunit.TraitAttribute("Description", "IIS-6370 - View and interact with data in profile in the objects section")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySectionUI")]
        public virtual void IIS_6370_ViewAndInteractWithDataInProfileInTheObjectsSection()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "ObjectOfStudySectionUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6370 - View and interact with data in profile in the objects section", null, tagsOfScenario, argumentsOfScenario);
#line 41
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
#line 42
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 43
        testRunner.And("I searched 3 омсбр data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 44
        testRunner.And("I clicked on the first search result title in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 45
        testRunner.And("I clicked on enlarge small card button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 46
        testRunner.And("I clicked on the Classifier block in the big card window", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 47
        testRunner.And("I clicked on the Direct reporting relationship link in the big card window", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 48
        testRunner.Then("I must see the title 1 армійський корпус in the small card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 49
        testRunner.When("I clicked on the General info block in the big card window", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 50
        testRunner.Then("I must see name real full is equal to the 3 окрема мотострілецька бригада Беркут " +
                        "value", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IS-5885 - Create a military organization")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySection - sanity")]
        [Xunit.TraitAttribute("Description", "IS-5885 - Create a military organization")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySectionUI")]
        public virtual void IS_5885_CreateAMilitaryOrganization()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "ObjectOfStudySectionUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IS-5885 - Create a military organization", null, tagsOfScenario, argumentsOfScenario);
#line 53
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
#line 54
        testRunner.When("I clicked on the create a new object of study button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 55
        testRunner.And("I clicked on the create a new military organization button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 56
        testRunner.And("I entered the джокер value in the affiliation field", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 57
        testRunner.And("I entered the першочерговий value in the importance field", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 58
        testRunner.And("I clicked on the classifiers block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 59
        testRunner.And("I entered the 28 обр РХБЗ value in the direct reporting relationship field", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 60
        testRunner.And("I clicked on the general info block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 61
        testRunner.And("I entered the 29-я окрема бригада РХБ захисту імені Героя Радянського Союзу генер" +
                        "ал-полковника В. К. Пікалова, в/ч 34081 value in the name real full field", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 62
        testRunner.And("I clicked on the dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 63
        testRunner.And("I entered the 48 value in the latitude field at the dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 64
        testRunner.And("I entered the 48 value in the longitude field at dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 65
        testRunner.And("I entered the Росія value in the country field at the dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 66
        testRunner.And("I clicked on the dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 67
        testRunner.And("I scrolled down to the Ідентифікаційні ознаки element", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 68
        testRunner.And("I clicked on the temporary dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 69
        testRunner.And("I entered the 49 value in the longitude field at temporary dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 70
        testRunner.And("I entered the 49 value in the latitude field at the temporary dislocation block", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 71
        testRunner.And("I entered the Росія value in the country field at the temporary dislocation block" +
                        "", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 72
        testRunner.And("I clicked on the save button to create a new object of study", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 73
        testRunner.And("I clicked on the confirm save button to create a new object of study", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
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
                ObjectsOfStudySection_SanityFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                ObjectsOfStudySection_SanityFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
