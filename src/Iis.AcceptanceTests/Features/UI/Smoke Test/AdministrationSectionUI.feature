Feature: AdministrationSectionUI - Smoke

	- Open Administration section

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@smoke @UI @EventsUI
	Scenario: Ensure that Administration section is opened
		When I navigated to Administration page
		Then I must see the Administration page
		Then I must see first user in the user list
