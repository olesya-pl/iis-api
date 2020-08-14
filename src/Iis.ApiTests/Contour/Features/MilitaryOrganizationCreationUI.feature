Feature: MilitaryOrganizationCreationUI

	- Create a new military organization
Background: 
	Given I want to sign in with the user olya and password hammer69 in the Contour

@smoke
Scenario: Create a new military organization
	And I select an element .el-tree-node:nth-child(2) in the .quick-filters__actions .add-button pop up