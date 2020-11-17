Feature: ObjectsSectionEmptySearchUI

	https://jira.infozahyst.com/browse/IIS-6210

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: Enter ! in the search field and get 0 results
	Given I click .entity-search__toggle .el-button--default button
	When I entered ! in the .entity-search__body .el-input__inner text field and press Enter key
	Then I must see the .infinity-table__no-results element

