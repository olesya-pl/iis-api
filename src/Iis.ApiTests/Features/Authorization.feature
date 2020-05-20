Feature: Authorization

@authorization
Scenario: Success login
	Given I want to authenticate with the user olya and password hammer69
	When When I send login request
	Then The result should contain authorization token

@authorization
Scenario: Invalid password
	Given I want to authenticate with the user olya and password 123456789
	When When I send incorrect login request
	Then The result should not contain authorization token
