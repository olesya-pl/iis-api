Feature: EventsTabUI - Smoke

	- IIS-6226 - Ensure that Events section is opened

Background: 
	Given I sign in with the user olya and password 123 in the Contour

@smoke @UI @EventsUI
Scenario: IIS-6226 - Ensure that Events section is opened
	When I navigated to Events page
	Then I must see the Events page
	Then I must see first event in the events list