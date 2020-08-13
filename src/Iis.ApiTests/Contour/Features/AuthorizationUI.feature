Feature: Authorization UI
	- Valid authorization
    #- Invalid authorization

#Background: 
	#Given I want to sign in with the user olya and password hammer69 in the Contour

@UITests @Authorization
Scenario: Authorize by using valid credentials
	Given I want to sign in with the user olya and password hammer69 in the Contour
	Then I see object page