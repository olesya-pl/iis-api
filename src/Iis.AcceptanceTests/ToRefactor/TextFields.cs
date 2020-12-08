using System;
using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace AcceptanceTests.ToRefactor
{
    [Binding]
    public class TextFields
    {
        private readonly ScenarioContext context;
        private readonly IWebDriver driver;

        public TextFields(ScenarioContext injectedContext)
        {
            context = injectedContext;

            driver = context.GetDriver();
        }

        [Given(@"I enter (.*) in the (.*) text field and add current date to the input")]
        public void GivenIEnterSomethingInTheTextField(string text, string textField)
        {
            IWebElement textForm = driver.FindElement(By.CssSelector(textField));
            textForm.SendKeys($"{text}_{DateTime.Now}");
        }

        [Given(@"I input (.*) in the (.*) text field")]
        public void GivenIInputInTheTextField(string text, string textField)
        {
            IWebElement textForm = driver.FindElement(By.CssSelector(textField));

            textForm.SendKeys(text);
        }

        [When(@"I entered (.*) in the (.*) text field and press Enter key")]
        public void GivenIInputInTheTextFieldAndPressEnterKey(string text, string textField)
        {
            IWebElement textForm = driver.FindElement(By.CssSelector(textField));

            textForm.SendKeys(text);
            textForm.SendKeys(Keys.Enter);
        }
       
    }
}
