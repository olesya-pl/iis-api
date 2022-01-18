Feature: ChatSectionUI - Smoke

	- IIS-8566 - Ensure that Chat can be opened

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@smoke @UI @ChatSectionUI
	Scenario: IIS-8566 - Ensure that Chat can be opened
		When I navigated on the Chat section
		Then I must see list of users for correspondence