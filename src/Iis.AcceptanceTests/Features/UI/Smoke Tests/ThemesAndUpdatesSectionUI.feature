Feature: ThemesAndUpdatesSectionUI
	https://jira.infozahyst.com/browse/IIS-6326

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: Themes And Updates section is opened
	Given I click li:nth-of-type(4) > .sidebar__nav-item-title button
	Then I must see the tbody > tr:nth-of-type(1) element in 15 seconds
