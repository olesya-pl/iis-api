Feature: WikiSectionUI - Smoke

	- IIS-7473 - Open the Wiki section

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@smoke @UI @WikiUI
	Scenario: Open the Wiki section
		When I navigated to the Wiki page
		Then I must see the Wiki page
