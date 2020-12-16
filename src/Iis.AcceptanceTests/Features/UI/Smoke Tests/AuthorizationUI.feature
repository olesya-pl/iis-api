Feature: Authorization UI

	- Valid authorization
	- Invalid authorization

	@smoke @UI @AuthorizationSmokeUI
	Scenario: Authorize by using valid credentials
		Given I sign in with the user olya and password 123 in the Contour
		Then I redirected to objects page

	@smoke @UI @AuthorizationSmokeUI
	Scenario: Try to authorize by using invalid credentials
		Given I sign in with the user olya and password hammer691 in the Contour
		Then Login button is active
		Then Login and password inputs are highlighted with red
		Then I see the error message that login or password is incorrect