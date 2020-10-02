Feature: Objects tab - Smoke

	https://jira.infozahyst.com/browse/IIS-6206

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: Ensure that Objects tab is opened
	Then I must see the tbody > tr:nth-of-type(1) element