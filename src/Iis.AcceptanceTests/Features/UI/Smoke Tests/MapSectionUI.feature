Feature: MapSection - smoke

	- IIS-6216 - Map section can be opened

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@smoke @UI @MapSectionUI
	Scenario: IIS-6140 - Map section can be opened
		When I navigated to Map page
		Then I must see Map block