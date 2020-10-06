Feature: IOBigCard - Smoke

	https://jira.infozahyst.com/browse/IIS-6208

Background: 
	Given I want to sign in with the user olya and password 123 in the Contour

@mytag
Scenario: Intelligence objective - big card is opened
	Given I click .el-table__row:nth-of-type(1) .text-ellipsis.title button
	And I click button[name='btn-full-screen'] button
	Then I must see the div[name='affiliation'] div[name='view-item-relation'] element