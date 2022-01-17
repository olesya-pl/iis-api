﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.7.0.0
//      SpecFlow Generator Version:3.7.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.7.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class MaterialsSectionUI_SanityFeature : object, Xunit.IClassFixture<MaterialsSectionUI_SanityFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "MaterialsSection.feature"
#line hidden
        
        public MaterialsSectionUI_SanityFeature(MaterialsSectionUI_SanityFeature.FixtureData fixtureData, AcceptanceTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/UI/Sanity Tests", "MaterialsSectionUI - Sanity", @"    - IIS-6375 - Material processing, priority and importance setup
    - IIS-6374 - ML results display for DOCX material
    - IIS-5837- Connect a material with an object of study from material
    - IIS-6363 - Search a material by keyword from the material
    - IIS-8102 - Go to Events page from the material
    - IIS-8105 - Go to Report page from the material", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 10
    #line hidden
#line 11
        testRunner.Given("I sign in with the user olya and password 123 in the Contour", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6375 - Material processing, priority and importance setup")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Sanity")]
        [Xunit.TraitAttribute("Description", "IIS-6375 - Material processing, priority and importance setup")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSanityUI")]
        public virtual void IIS_6375_MaterialProcessingPriorityAndImportanceSetup()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "MaterialsSanityUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6375 - Material processing, priority and importance setup", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
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
#line 10
    this.FeatureBackground();
#line hidden
#line 15
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 16
        testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 17
        testRunner.And("I set importance Друга категорія value", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 18
        testRunner.And("I set reliability Достовірна value", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 19
        testRunner.And("I pressed Processed button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 20
        testRunner.When("I pressed Back button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 21
        testRunner.Then("I must see that importance value must be set to Друга категорія value", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 22
        testRunner.Then("I must see that reliability value must be set to Достовірна value", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 23
        testRunner.When("I close the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6374 - ML results display for DOCX material")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Sanity")]
        [Xunit.TraitAttribute("Description", "IIS-6374 - ML results display for DOCX material")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSanityUI")]
        public virtual void IIS_6374_MLResultsDisplayForDOCXMaterial()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "MaterialsSanityUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6374 - ML results display for DOCX material", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
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
#line 10
    this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                            "Field",
                            "Value"});
                table9.AddRow(new string[] {
                            "FileName",
                            "тестовий матеріал"});
                table9.AddRow(new string[] {
                            "SourceReliabilityText",
                            "Здебільшого надійне"});
                table9.AddRow(new string[] {
                            "ReliabilityText",
                            "Достовірна"});
                table9.AddRow(new string[] {
                            "Content",
                            "таємний контент"});
                table9.AddRow(new string[] {
                            "AccessLevel",
                            "0"});
                table9.AddRow(new string[] {
                            "LoadedBy",
                            "автотест"});
                table9.AddRow(new string[] {
                            "MetaData",
                            "{\"type\":\"document\",\"source\":\"contour.doc\"}"});
#line 27
        testRunner.Given("I upload a new docx material via API", ((string)(null)), table9, "Given ");
#line hidden
#line 36
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 37
        testRunner.And("I clicked search button in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 38
        testRunner.And("I searched таємн data in the materials", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 39
        testRunner.And("I clicked on the first search result in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 40
        testRunner.And("I clicked on the ML tab in the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 41
        testRunner.And("I pressed Show button to show Text classifier ML output", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 42
        testRunner.Then("I must see Text classifier ML output form", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 43
        testRunner.When("I close the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 44
        testRunner.And("I clean up uploaded material via API", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-5837- Connect a material with an object of study from material")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Sanity")]
        [Xunit.TraitAttribute("Description", "IIS-5837- Connect a material with an object of study from material")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSanityUI")]
        public virtual void IIS_5837_ConnectAMaterialWithAnObjectOfStudyFromMaterial()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "MaterialsSanityUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-5837- Connect a material with an object of study from material", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 47
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
#line 10
    this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                            "Field",
                            "Value"});
                table10.AddRow(new string[] {
                            "FileName",
                            "тестовий матеріал"});
                table10.AddRow(new string[] {
                            "SourceReliabilityText",
                            "Здебільшого надійне"});
                table10.AddRow(new string[] {
                            "ReliabilityText",
                            "Достовірна"});
                table10.AddRow(new string[] {
                            "Content",
                            "таємний контент"});
                table10.AddRow(new string[] {
                            "AccessLevel",
                            "0"});
                table10.AddRow(new string[] {
                            "LoadedBy",
                            "автотест"});
                table10.AddRow(new string[] {
                            "MetaData",
                            "{\"type\":\"document\",\"source\":\"contour.doc\"}"});
#line 48
         testRunner.Given("I upload a new docx material via API", ((string)(null)), table10, "Given ");
#line hidden
#line 57
        testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 58
        testRunner.And("I clicked search button in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 59
        testRunner.And("I searched таємн data in the materials", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 60
        testRunner.And("I clicked on the first search result in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 61
        testRunner.And("I clicked on the relations tab in the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 62
        testRunner.And("I enter Романов value in the search object field", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 63
        testRunner.When("I clicked on the binded object", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 64
        testRunner.Then("I must see РОМАНОВ А.Г title of the object", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 65
        testRunner.When("I clicked Back button in the browser", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 66
        testRunner.And("I clicked on the relations tab in the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 67
        testRunner.And("I clicked on the delete button to destroy relation between the material and the Р" +
                        "ОМАНОВ А.Г object", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 68
        testRunner.When("I pressed the confirm button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 69
        testRunner.Then("I must not see the related РОМАНОВ А.Г object in the material", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 70
        testRunner.When("I close the material card", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 71
        testRunner.And("I clean up uploaded material via API", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-6363 - Search a material by keyword from the material")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Sanity")]
        [Xunit.TraitAttribute("Description", "IIS-6363 - Search a material by keyword from the material")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSanityUI")]
        public virtual void IIS_6363_SearchAMaterialByKeywordFromTheMaterial()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "MaterialsSanityUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-6363 - Search a material by keyword from the material", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 74
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
#line 10
    this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                            "Field",
                            "Value"});
                table11.AddRow(new string[] {
                            "FileName",
                            "тестовий матеріал"});
                table11.AddRow(new string[] {
                            "SourceReliabilityText",
                            "Здебільшого надійне"});
                table11.AddRow(new string[] {
                            "ReliabilityText",
                            "Достовірна"});
                table11.AddRow(new string[] {
                            "Content",
                            "таємний контент"});
                table11.AddRow(new string[] {
                            "AccessLevel",
                            "0"});
                table11.AddRow(new string[] {
                            "LoadedBy",
                            "автотест"});
                table11.AddRow(new string[] {
                            "MetaData",
                            "{\"type\":\"document\",\"source\":\"contour.doc\"}"});
#line 75
        testRunner.Given("I upload a new docx material via API", ((string)(null)), table11, "Given ");
#line hidden
#line 84
 testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 85
 testRunner.And("I clicked search button in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 86
 testRunner.And("I searched таємн data in the materials", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 87
    testRunner.Then("I must see a material that contains таємн word in the Materials search result", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 88
    testRunner.When("I clean up uploaded material via API", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-8102 - Go to Events page from the material")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Sanity")]
        [Xunit.TraitAttribute("Description", "IIS-8102 - Go to Events page from the material")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSanityUI")]
        public virtual void IIS_8102_GoToEventsPageFromTheMaterial()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "MaterialsSanityUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-8102 - Go to Events page from the material", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 92
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
#line 10
    this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                            "Field",
                            "Value"});
                table12.AddRow(new string[] {
                            "FileName",
                            "тестовий матеріал"});
                table12.AddRow(new string[] {
                            "SourceReliabilityText",
                            "Здебільшого надійне"});
                table12.AddRow(new string[] {
                            "ReliabilityText",
                            "Достовірна"});
                table12.AddRow(new string[] {
                            "Content",
                            "таємний контент"});
                table12.AddRow(new string[] {
                            "AccessLevel",
                            "0"});
                table12.AddRow(new string[] {
                            "LoadedBy",
                            "автотест"});
                table12.AddRow(new string[] {
                            "MetaData",
                            "{\"type\":\"document\",\"source\":\"contour.doc\"}"});
#line 93
        testRunner.Given("I upload a new docx material via API", ((string)(null)), table12, "Given ");
#line hidden
#line 102
 testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 103
 testRunner.And("I clicked search button in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 104
 testRunner.And("I searched таємн data in the materials", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 105
    testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 106
    testRunner.When("I clicked on the Events section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 107
 testRunner.Then("I must see first event in the events list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 108
    testRunner.When("I clean up uploaded material via API", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="IIS-8105 - Go to Report page from the material")]
        [Xunit.TraitAttribute("FeatureTitle", "MaterialsSectionUI - Sanity")]
        [Xunit.TraitAttribute("Description", "IIS-8105 - Go to Report page from the material")]
        [Xunit.TraitAttribute("Category", "sanity")]
        [Xunit.TraitAttribute("Category", "UI")]
        [Xunit.TraitAttribute("Category", "MaterialsSanityUI")]
        public virtual void IIS_8105_GoToReportPageFromTheMaterial()
        {
            string[] tagsOfScenario = new string[] {
                    "sanity",
                    "UI",
                    "MaterialsSanityUI"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("IIS-8105 - Go to Report page from the material", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 112
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
#line 10
    this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                            "Field",
                            "Value"});
                table13.AddRow(new string[] {
                            "FileName",
                            "тестовий матеріал"});
                table13.AddRow(new string[] {
                            "SourceReliabilityText",
                            "Здебільшого надійне"});
                table13.AddRow(new string[] {
                            "ReliabilityText",
                            "Достовірна"});
                table13.AddRow(new string[] {
                            "Content",
                            "таємний контент"});
                table13.AddRow(new string[] {
                            "AccessLevel",
                            "0"});
                table13.AddRow(new string[] {
                            "LoadedBy",
                            "автотест"});
                table13.AddRow(new string[] {
                            "MetaData",
                            "{\"type\":\"document\",\"source\":\"contour.doc\"}"});
#line 113
        testRunner.Given("I upload a new docx material via API", ((string)(null)), table13, "Given ");
#line hidden
#line 122
 testRunner.When("I navigated to Materials page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 123
 testRunner.And("I clicked search button in the Materials section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 124
 testRunner.And("I searched таємн data in the materials", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 125
    testRunner.And("I clicked on the first material in the Materials list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 126
    testRunner.When("I navigated to Report section", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 127
 testRunner.Then("I must see first report in the report list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 128
    testRunner.When("I clean up uploaded material via API", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.7.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                MaterialsSectionUI_SanityFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                MaterialsSectionUI_SanityFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
