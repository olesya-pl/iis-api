Feature: ThemesAndUpdate - smoke

	- IIS-6326 - Themes and updates section can be opened

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@smoke @UI @ThemesAndUpdatesSectionUI
	Scenario: IS-6326 - Themes and updates section can be opened
		When I navigated to Themes and updates section
		Then I must see first theme in the Themes list