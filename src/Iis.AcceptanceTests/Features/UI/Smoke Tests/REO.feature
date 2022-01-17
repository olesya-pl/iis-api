Feature: REO - Smoke

	- IIS-8554 - REO section can be opened

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@smoke @UI @REO
	Scenario: IIS-8554 - REO section can be opened
		When I navigated to Reo page
		Then I must see Reo block
