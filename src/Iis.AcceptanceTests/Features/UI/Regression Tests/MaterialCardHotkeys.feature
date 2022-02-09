Feature: Material_HotKeys - regression
	- IIS-8238 - Hotkeys for audio rewind
	- IIS-8257 - Possibility save  material by hotkeys
	- IIS-8240 - Possibility to change the priority for materials using hotkeys
	- IIS-7314 - Hotkeys for audio play/pause audio materials

Background:
	Given I sign in with the user olya and password 123 in the Contour

@regression @UI @Material_HotKeys
Scenario: IIS-8238 - Hotkeys for audio rewind
	When I navigated to Materials page
	Then I clicked on the type`s filter audio
	When I clicked search button in the Materials section
	And I searched "neizvesten-peregovory-po-racii.mp3" data in the materials
	And I clicked on the first material in the Materials list
	And I clicked pause button
	Then I clicked on the text field
	When I clicked Ctrl and left arrow on the keyboard
	Then I see that position of timeline changed
	And I clicked Ctrl and right arrow on the keyboard

	@regression @UI @Material_HotKeys
Scenario: IIS-8257 - Possibility save  material by hotkeys
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
	Then I wrote on the text field  Якийсь текст
	When I send keys Ctrl and [ from the keyboard
	Then I must see notification about saving material
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must see saved new content with text  Якийсь текст in the text field
	When I clean up uploaded material via API

	@regression @UI @Material_HotKeys
Scenario: IIS-8240 - Possibility to change the priority for materials using hotkeys
		 Given I upload a new docx material via API
		| Field                 | Value                                        |
		| FileName              | тестовий матеріал                            |
		| SourceReliabilityText | Здебільшого надійне                          |
		| ReliabilityText       | Достовірна                                   |
		| Content               | таємний контент                              |
		| AccessLevel           | 0                                            |
		| LoadedBy              | автотест                                     |
		| MetaData              | {"type":"document","source":"contour.doc"} |
		| From                  | contour.doc                                |
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched таємн data in the materials
	And I clicked on the first material in the Materials list
	When I press hotkeys Ctrl+Alt+2
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Important
	When I press hotkeys Ctrl+Alt+3
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Immediate Report
	When I press hotkeys Ctrl+Alt+4
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Translation
	When I clean up uploaded material via API

	@regression @UI @Material_HotKeys
Scenario: IIS-7314 - Hotkeys for audio play/pause audio materials
	When I navigated to Materials page
	Then I clicked on the type`s filter audio
	When I clicked search button in the Materials section
	And I searched neizvesten-peregovory-po-racii.mp3 data in the materials
	And I clicked on the first material in the Materials list
	When I press hotkeys Ctrl+Space
	Then I must see the play/pause button in look like arrow
	When I press hotkeys Ctrl+Space
	Then I must see the play/pause button in look like pause