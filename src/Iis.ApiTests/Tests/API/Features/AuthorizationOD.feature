Feature: Authorization in the Odysseus

@authorization
Scenario: Success login
	Given I want to authenticate with the user olya and password hammer69 in the Odysseus
	When I send login request to the Odysseus
	Then The result should contain authorization token from the Odysseus

@authorization
Scenario: Invalid password
	Given I want to authenticate with the user olya111 and password 123456789 in the Odysseus
	When I send login request to the Odysseus
	Then The result should not contain authorization token from the Odysseus
