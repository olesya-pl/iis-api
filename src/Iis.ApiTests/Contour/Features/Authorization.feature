Feature: Authorization

@authorization
Scenario: Success login
	Given I want to authenticate with the user olya and password hammer69 in the Contour
	When I send login request to the Contour
	Then The result should contain authorization token from the Countour

@authorization
Scenario: Invalid password
	Given I want to authenticate with the user olya111 and password 123456789 in the Contour
	When I send login request to the Contour
	Then The result should not contain authorization token from the Contour
