Feature: Materials - regression
	- IIS-6470 - Sorting materials by source
	- IIS-6473 - Sorting materials by importance
	- IIS-6477 - Sorting materials by created date

Background:
	Given I sign in with the user olya and password 123 in the Contour

@regression @UI @Material @upload
Scenario: IIS-6470 - Sorting materials by source
		Given I upload a new docx material via API
		| Field                 | Value                                        |
		| FileName              | тестовий матеріал                            |
		| SourceReliabilityText | Здебільшого надійне                          |
		| ReliabilityText       | Достовірна                                   |
		| Content               | таємний контент                              |
		| AccessLevel           | 0                                            |
		| LoadedBy              | автотест                                     |
		| MetaData              | {"type":"document","source":"a.contour.doc"} |
		| From                  | a.contour.doc                                |
	When I navigated to Materials page
	When I clicked arrow for sorting by source
	Then I must see the first a.contour.doc source in the marerial`s table 
	When I clean up uploaded material via API
		Given I upload a new docx material via API
		| Field                 | Value                                        |
		| FileName              | тестовий матеріал                            |
		| SourceReliabilityText | Здебільшого надійне                          |
		| ReliabilityText       | Достовірна                                   |
		| Content               | таємний контент                              |
		| AccessLevel           | 0                                            |
		| LoadedBy              | автотест                                     |
		| MetaData              | {"type":"document","source":"z.contour.doc"} |
		| From                  | z.contour.doc                                |
	When I clicked arrow for sorting by source
	Then I must see the first z.contour.doc source in the marerial`s table
	When I clicked arrow for sorting by source
	Then I must see materials sorted by source sorting: null
	When I clean up uploaded material via API

@regression @UI @Materials
Scenario: IIS-6473 - Sorting materials by importance
	When I navigated to Materials page
	When I clicked arrow for sorting by importance
	Then I must see more important materials at the top of the table
	When I clicked arrow for sorting by importance
	Then I must see less important materials at the top of the table
	When I clicked arrow for sorting by importance
	Then I must see materials sorted by importance sorting: null

		@regression @UI @Materials
Scenario: IIS-6477 - Sorting materials by Created Date
	When I navigated to Materials page
	Then I must see created date in descending order
	When I clicked arrow for sorting by created date
	Then I must see materials sorted by created date sorting null
	When I clicked arrow for sorting by created date
	Then I must see created date in ascending order