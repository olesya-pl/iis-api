Feature: Authorization UI

	- Valid authorization


@smoke
Scenario: Authorize by using valid credentials
	Given I want to sign in with the user olya and password hammer69 in the Contour
	Then I see the http://qa.contour.net/objects/?page=1 link in the browser navigation bar