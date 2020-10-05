Feature: Authorization UI

	- Valid authorization
	- Invalid authorization


@smoke
Scenario: Authorize by using valid credentials
	Given I want to sign in with the user olya and password 123 in the Contour
	Then I see the http://qa.contour.net/objects/?page=1 link in the browser navigation bar

@smoke
Scenario: Try to authorize by using invalid credentials
	Given I want to sign in with the user olya and password hammer691 in the Contour
	Then the button .transition-box must be active
	Then the text field div[name='username']  .el-input.el-input--small.has-error.is-dark and text field div[name='password']  .el-input.el-input--small.has-error.is-dark must be highlighted with red color
	Then I must see the specific text Пароль або імʼя користувача вказані невірно. in the text .error-message block on the page