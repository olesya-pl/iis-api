Feature: UserCreation
 - Create a new user in the administration section

@mytag
Scenario: Create a new user
	Given I want to sign in with the user olya and password hammer69 in the Contour
	And I click .sidebar__nav-item[name="admin"] button
	And I click .users-page__header .add-button button
	And I complete the userform with TestUser, TestSurname, TestPatronym, TestUserAuto, TestPassW00rd and TestPassW00rd
	And I choose element from dropdown menu
	And I click .role-editor__footer .el-button--primary button
	#And I click Підтвердити button in pop-up
	#Then the result should be 120

	#Examples: 
	#| firstname | lastname    | patronym     | username     | password      | conformationPassword |
	#| TestUser  | TestSurname | TestPatronym | TestUserAuto | TestPassW00rd | TestPassW00rd        |