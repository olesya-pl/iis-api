Feature: Materials

@materials
Scenario: Get paged materials
	Given Authorized user with login olya and password 123
	When I want to request materials for page 1 and get 10 materials per page
	Then the result should contain 10 items in the response

@materials
Scenario: Update material importance
	Given Authorized user with login olya and password 123
	When I want to set importance 1356a6b3c63f49858b74372236fe744f for the first material on page
	Then update should be executed without errors