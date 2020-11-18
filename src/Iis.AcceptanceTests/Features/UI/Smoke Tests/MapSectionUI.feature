Feature: MapSectionUI
	https://jira.infozahyst.com/browse/IIS-6216

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: Ensure that Objects tab is opened
	Given I click div:nth-of-type(1) > li:nth-of-type(5) button
	Then I must see the .measurement-widget > button[title='Інструмент вимірювання площі'] element in 15 seconds