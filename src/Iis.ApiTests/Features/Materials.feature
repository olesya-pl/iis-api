Feature: Materials

@materials
Scenario: Get paged materials
	Given Authorized user with login olya and password hammer69
	When I want to request materials fro page 1 and get 10 materials per page
	Then the result should contain 10 items in the response
