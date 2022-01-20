using System;
using System.Linq;
using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class ChatSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;
        private ChatPage ChatPage;
        private readonly NavigationSection navigationSection;

        public ChatSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            ChatPage = new ChatPage(driver);
            navigationSection = new NavigationSection(driver);
            context = injectedContext;
            this.driver = driver;
        }

        #region When

        [When(@"I navigated on the Chat section")]
        public void INavigatedOnTheChatSection()
        {
            driver.WaitFor(2);
            navigationSection.ChatLink.Click();
        }

    #endregion

    #region Then

    [Then(@"I must see list of users for correspondence")]
    public void IMustSeeListOfUsersForCorrespondence()
    {
        driver.WaitFor(2);
        Assert.True(ChatPage.ListOfUsers.Displayed);
    }
        #endregion         
    }
}