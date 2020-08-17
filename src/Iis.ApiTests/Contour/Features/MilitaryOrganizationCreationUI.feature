Feature: MilitaryOrganizationCreationUI

	- Create a new military organization

Background: 
	Given I want to sign in with the user olya and password hammer69 in the Contour

@smoke @ignore
Scenario: Create a new military organization
	And I select an element .el-tree-node:nth-child(2) in the .quick-filters__actions .add-button pop up
	And I input очікує розгляду in the div[name='affiliation'] .el-input__inner text field
	And I press Down button plus Enter button on the div[name='affiliation'] .el-input__inner item
	And I input ігнорувати in the div[name='importance'] .el-input__inner text field
	And I press Down button plus Enter button on the div[name='importance'] .el-input__inner item
	And I click div[name='6b1997fc9d954ccf9c55ebf104b42986'] > div[role='button'] button
	And I enter e2e_Військовий підрозділ in the textarea[name='RealNameFull'] text field and add current date to the input
	And I click button[name='btn-save'] > span button
	When I press Enter button on the .el-button--default.el-button--primary span button
	Then I must see the button[name='btn-edit'] img button