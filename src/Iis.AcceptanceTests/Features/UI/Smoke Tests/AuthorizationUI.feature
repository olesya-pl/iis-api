Feature: Authorization UI

	- Authorize by using valid credentials
	- IIS-5795 - Invalid authorization
	- IIS-5796 - Authorize by using valid credentials

	@smoke @UI @AuthorizationSmokeUI
	Scenario: IIS-5796 - Authorize by using valid credentials
		Given I sign in with the user olya and password 123 in the Contour
		Then I redirected to objects page

	@smoke @UI @AuthorizationSmokeUI
	Scenario: IIS-5795 - Try to authorize by using invalid credentials
		Given I sign in with the user olya and password hammer691 in the Contour
		Then Login button is active
		Then Login and password inputs are highlighted with red