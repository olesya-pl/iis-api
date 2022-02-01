Feature: SearchMaterials - Regression

	- IIS-5953 - Possibility to search a material by a creation date
	- IIS-6633 - Search materials by status processing
	- IIS-6632 - Possibility searching materials by importance

	Background:
		Given I sign in with the user olya and password 123 in the Contour

@regression @UI @MaterialsSearchUI
Scenario: IIS-5953 - Possibility to search a material by a creation date
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched CreatedDate: 07.06.2021 data in the materials
	Then I must see the NATO PUBLIC DIPLOMACY PROGRAMMES title of the material
	When I clicked on the clear search button
	And I clicked search button in the Materials section
	And I searched CreatedDate: 07,06,2021 data in the materials
	Then I must see the NATO PUBLIC DIPLOMACY PROGRAMMES title of the material
	When I clicked on the clear search button
	And I clicked search button in the Materials section
	And I searched CreatedDate: 2021.06.07 data in the materials
	Then I must see the NATO PUBLIC DIPLOMACY PROGRAMMES title of the material
	When I clicked on the clear search button
	And I clicked search button in the Materials section
	And I searched CreatedDate: 2021,06,07 data in the materials
	Then I must see the NATO PUBLIC DIPLOMACY PROGRAMMES title of the material

		@regression @UI @Materials
Scenario: IIS-6633 - Search materials by status processing
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched ProcessedStatus.Title:Оброблено data in the materials
	Then I must see Оброблено in the marerial table

	@regression @UI @Materials
Scenario: IIS-6203 - Possibility To make text in bold
        Given I upload a new docx material via API
		| Field                 | Value                                      |
		| FileName              | тестовий матеріал                          |
		| SourceReliabilityText | Здебільшого надійне                        |
		| ReliabilityText       | Достовірна                                 |
		| Content               | таємний контент                           |
		| AccessLevel           | 0                                          |
		| LoadedBy              | автотест                                   |
		| MetaData              | {"type":"document","source":"contour.doc"} |
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched таємн data in the materials
	And I clicked on the first material in the Materials list
	Then I clicked on the text field
	And I highlight the text
	When I clicked on the editor button for bold
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must i see my text highlighted in bold
	When I clean up uploaded material via API

	@regression @UI @Materials
Scenario: IIS-6632 - Possibility searching materials by importance
	When I navigated to Materials page
	And I clicked search button in the Materials section
	When I searched by field name SessionPriority.Title: and request Важливий in the materials
	Then I must see list of the materials with SessionPriority.Title:Важливий
	When I clicked on the clear search button
	When I searched by field name SessionPriority.Title: and request Переклад in the materials
	Then I must see list of the materials with SessionPriority.Title:Переклад
	When I clicked on the clear search button
	When I searched by field name SessionPriority.Title: and request Негайна* in the materials
	Then I must see list of the materials with SessionPriority.Title:Негайна доповідь