Feature: EventsTabUI - Smoke

	https://jira.infozahyst.com/browse/IIS-6226

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: Ensure that Events section is opened
	And I click .sidebar__body .sidebar__nav:nth-of-type(1) .sidebar__nav-item:nth-of-type(2) .sidebar__nav-item-title button
	Then I must see the tbody > tr:nth-of-type(1) element