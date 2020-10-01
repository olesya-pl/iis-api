Feature: ReportSectionUI - Smoke

	https://jira.infozahyst.com/browse/IIS-6325

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: Ensure that Reports section is opened
	And I click div:nth-of-type(1) > li:nth-of-type(3) button
	Then I must see the .infinity-table > div:nth-of-type(1) element