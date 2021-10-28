Feature: Report - smoke

	- IIS-6325 - Report section can be opened

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@smoke @UI @ReportSectionUI
	Scenario: IIS-6325 - Report section can be opened
		When I navigated to Report section
		Then I must see first report in the report list