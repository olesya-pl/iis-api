Feature: IOSmallCard - Smoke

	https://jira.infozahyst.com/browse/IIS-6207

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@smoke
Scenario: Intelligence objective - small card is opened
	Given I click .el-table__row:nth-of-type(1) .text-ellipsis.title button
	Then I must see the .aside-card window