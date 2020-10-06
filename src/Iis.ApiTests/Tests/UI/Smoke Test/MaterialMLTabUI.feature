Feature: MaterialMLTabUI - Smoke

	https://jira.infozahyst.com/browse/IIS-6190

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: ML tab contains ML output
	Given I click li:nth-of-type(8) > .sidebar__nav-item-title button
	And I click tbody .el-table__row:nth-of-type(1) button
	And I click ul[role='menubar'] > li:nth-of-type(2) button
	Then I must see the div:nth-of-type(1) > .meta-data-card > .meta-data-expand element
