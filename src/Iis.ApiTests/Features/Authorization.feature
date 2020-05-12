Feature: Authorization

@authorization
Scenario: Success login
	Given I want to authenticate with the user olya and password hammer69
	When When I send login request
	Then The result should contain authorization token