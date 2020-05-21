Feature: Authorization

@authorization
Scenario: Success login
	Given I want to authenticate with the user olya and password hammer69
	When I send login request
	Then The result should contain authorization token

@authorization
Scenario: Invalid password
	Given I want to authenticate with the user olya111 and password 123456789
	When I send login request
	Then The result should not contain authorization token
