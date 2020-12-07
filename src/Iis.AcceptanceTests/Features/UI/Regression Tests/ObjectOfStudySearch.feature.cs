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
namespace AcceptanceTests.Features.UI.RegressionTests
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class ObjectsOfStudySearch_FunctionalFeature : object, Xunit.IClassFixture<ObjectsOfStudySearch_FunctionalFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ObjectOfStudySearch.feature"
#line hidden
        
        public ObjectsOfStudySearch_FunctionalFeature(ObjectsOfStudySearch_FunctionalFeature.FixtureData fixtureData, AcceptanceTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/UI/Regression Tests", "ObjectsOfStudySearch - functional", @"    - IIS-6140 - Search by two criteria by using OR operator
    - IIS-6139 - Search by two criteria by using NOT operator
    - IIS-6138 - Search by two criteria by using AND operator
    - IIS-6082 - Search object of study by full name
    - IIS-6207 - Open a small object of study card", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 9
    #line hidden
#line 10
        testRunner.Given("I sign in with the user olya and password 123 in the Contour", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6140 - Search by two criteria by using OR operator")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySearch - functional")]
        [Xunit.TraitAttribute("Description", "IIS-6140 - Search by two criteria by using OR operator")]
        [Xunit.TraitAttribute("Category", "functional")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectsOfStudySearchUI")]
        public virtual void IIS_6140_SearchByTwoCriteriaByUsingOROperator()
        {
            string[] tagsOfScenario = new string[] {
                    "functional",
                    "UI",
                    "ObjectsOfStudySearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6140 - Search by two criteria by using OR operator", null, tagsOfScenario, argumentsOfScenario);
#line 14
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
#line 9
    this.FeatureBackground();
#line hidden
#line 15
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 16
        testRunner.And("I searched Олександр OR Іванович data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 17
        testRunner.Then("I must see object of study ОТРОЩЕНКО Олександр Іванович as first search result", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6139 - Search by two criteria by using NOT operator")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySearch - functional")]
        [Xunit.TraitAttribute("Description", "IIS-6139 - Search by two criteria by using NOT operator")]
        [Xunit.TraitAttribute("Category", "functional")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectsOfStudySearchUI")]
        public virtual void IIS_6139_SearchByTwoCriteriaByUsingNOTOperator()
        {
            string[] tagsOfScenario = new string[] {
                    "functional",
                    "UI",
                    "ObjectsOfStudySearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6139 - Search by two criteria by using NOT operator", null, tagsOfScenario, argumentsOfScenario);
#line 20
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
#line 9
    this.FeatureBackground();
#line hidden
#line 21
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 22
        testRunner.And("I searched Олександр NOT Іванович data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 23
        testRunner.Then("I must not see object of study ОТРОЩЕНКО Олександр Іванович as first search resul" +
                        "t", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6138 - Search by two criteria by using AND operator")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySearch - functional")]
        [Xunit.TraitAttribute("Description", "IIS-6138 - Search by two criteria by using AND operator")]
        [Xunit.TraitAttribute("Category", "functional")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectsOfStudySearchUI")]
        public virtual void IIS_6138_SearchByTwoCriteriaByUsingANDOperator()
        {
            string[] tagsOfScenario = new string[] {
                    "functional",
                    "UI",
                    "ObjectsOfStudySearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6138 - Search by two criteria by using AND operator", null, tagsOfScenario, argumentsOfScenario);
#line 26
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
#line 9
    this.FeatureBackground();
#line hidden
#line 27
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 28
        testRunner.And("I searched Ткачук AND \"3 омсбр\" data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 29
        testRunner.Then("I must see object of study ТКАЧУК Руслан Юрійович as first search result", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 30
        testRunner.Then("I must see search results counter value that equal to 1 value", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6082 - Search object of study by full name")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySearch - functional")]
        [Xunit.TraitAttribute("Description", "IIS-6082 - Search object of study by full name")]
        [Xunit.TraitAttribute("Category", "functional")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectsOfStudySearchUI")]
        public virtual void IIS_6082_SearchObjectOfStudyByFullName()
        {
            string[] tagsOfScenario = new string[] {
                    "functional",
                    "UI",
                    "ObjectsOfStudySearchUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6082 - Search object of study by full name", null, tagsOfScenario, argumentsOfScenario);
#line 33
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
#line 9
    this.FeatureBackground();
#line hidden
#line 34
        testRunner.When("I clicked on search button in the Object of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 35
        testRunner.And("I searched в/ч 85683-А data in the Objects of study section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 36
        testRunner.Then("I must see object of study радіотехнічний батальойн в/ч 85683-А as first search r" +
                        "esult", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6207 - Open a small object of study card")]
        [Xunit.TraitAttribute("FeatureTitle", "ObjectsOfStudySearch - functional")]
        [Xunit.TraitAttribute("Description", "IIS-6207 - Open a small object of study card")]
        [Xunit.TraitAttribute("Category", "smoke")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "ObjectOfStudySmallCardUI")]
        public virtual void IIS_6207_OpenASmallObjectOfStudyCard()
        {
            string[] tagsOfScenario = new string[] {
                    "smoke",
                    "UI",
                    "ObjectOfStudySmallCardUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6207 - Open a small object of study card", null, tagsOfScenario, argumentsOfScenario);
#line 39
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
#line 9
    this.FeatureBackground();
#line hidden
#line 40
        testRunner.When("I clicked on first object of study", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 41
        testRunner.Then("I must see the object of study small card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
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
                ObjectsOfStudySearch_FunctionalFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                ObjectsOfStudySearch_FunctionalFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion