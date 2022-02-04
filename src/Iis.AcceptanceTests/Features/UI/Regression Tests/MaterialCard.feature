Feature: MaterialCard - regression
	- IIS-6045 - Change material reliability by clicking on the Processed button
	- IIS-6048 - Change a material priority by clicking on the Processed button
	- IIS-6051 - Ability to connect the material to an event from a material
	- IIS-6052 - Ability to lose the connection between a material and an event from a material
	- IIS-6109 - Indicate a phone number pattern of a cell voice material
	- IIS-6203 - Possibility To make text in bold
	- IIS-7445 - Change a material`s AccessLevel
	- IIS-8441 - Display the length of the audio track
	- IIS-6197 - Possibility change priority for audio materials

Background:
	Given I sign in with the user olya and password 123 in the Contour

@regression @UI @MaterialCardCard
Scenario: IIS-6109 - Indicate a phone number pattern of a cell voice material
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched "Voice_158(158)" data in the materials
	And I clicked on the first material in the Materials list
	When I clicked on the pattern tab
	Then I must see that phone number pattern is equal to value
		"""

		"+380713176787",

		"""

@regression @UI @MaterialCardCard
Scenario: IIS-6048 - Change a material priority by clicking on the Processed button
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched таємний data in the materials
	And I clicked on the first material in the Materials list
	And I set the session priority to Important
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Important
	When I set the session priority to Immediate Report
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Immediate Report
	When I set the session priority to Translation
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Translation

@regression @UI @MaterialCardCard
Scenario: IIS-6045 - Change a material reliability by clicking on the Processed button
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched таємний data in the materials
	And I clicked on the first material in the Materials list
	And I set the source credibility Повністю надійне value
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the source credibility value must be set to the Повністю надійне value
	#When I set the source credibility Здебільшего надійне value
	#And I pressed Processed button
	#When I pressed the Previous material button
	#Then I must see that the source credibility value must be set to the Здебільшего надійне value
	When I set the source credibility Відносно надійне value
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the source credibility value must be set to the Відносно надійне value
	When I set the source credibility Не завжди надійне value
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the source credibility value must be set to the Не завжди надійне value
	When I set the source credibility Ненадійне value
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the source credibility value must be set to the Ненадійне value
	When I set the source credibility Неможливо оцінити надійність value
	And I pressed Processed button
	When I pressed the Previous material button
	Then I must see that the source credibility value must be set to the Неможливо оцінити надійність value

@regression @UI @MaterialCardCard
Scenario: IIS-6052 - Ability to lose the connection between a material and an event from a material
	When I navigated to Events page
	And I created a new Тестова подія event
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched таємний data in the materials
	And I clicked on the first material in the Materials list
	And I clicked on the relations tab in the material card
	And I binded the Тестова подія event to the material
	And I clicked on the delete button to destroy relation between the material and the Тестова подія event
	When I pressed the confirm button
	Then I must not see Тестова подія as the related event to the material

@regression @UI @MaterialCard
Scenario: IIS-6051 - Ability to connect the material to an event from a material section
	When I navigated to Events page
	And I created a new Тестова подія event
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched таємний data in the materials
	And I clicked on the first material in the Materials list
	And I clicked on the relations tab in the material card
	When I binded the Тестова подія event to the material
	Then I must see Тестова подія as the related event to the material
	When I clicked on the delete button to destroy relation between the material and the Тестова подія event
	When I pressed the confirm button
	Then I must not see Тестова подія as the related event to the material

@regression @UI @MaterialCard @upload
Scenario: I can upload material and find it by its name
	Given I upload a new docx material via API
		| Field                 | Value                                      |
		| FileName              | тестовий матеріал                          |
		| SourceReliabilityText | Здебільшого надійне                        |
		| ReliabilityText       | Достовірна                                 |
		| Content               | тестовий контент                           |
		| AccessLevel           | 0                                          |
		| LoadedBy              | автотест                                   |
		| MetaData              | {"type":"document","source":"contour.doc"} |
	When I navigated to Materials page
	And I clicked search button in the Materials section
	And I searched for uploaded material in the materials
	And I clicked on the first material in the Materials list
	And I clean up uploaded material via API

@regression @UI @MaterialCard @upload
Scenario: IIS-7445 - Change a material`s AccessLevel 
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
		And I entered the Т - Таємно value in the accessLevel field 
		Then I must see Т - Таємно value in the accessLevel field
		When I clean up uploaded material via API

@regression @UI @MaterialCards
Scenario: IIS-8441 - Display the length of the audio track
	When I navigated to Materials page
	When I clicked search button in the Materials section
	And I searched "neizvesten-peregovory-po-racii.mp3" data in the materials
	And I clicked on the material-type icon
	Then I must see player controls panel with total time
	When I clicked on the first material in the Materials list
	Then I must see the length of the audio track
	When I close the material card

@regression @UI @MaterialCard @upload
Scenario: IIS-6197 - Possibility  change priority for audio materials
	When I navigated to Materials page
	Then I clicked on the type`s filter audio
	When I clicked search button in the Materials section
	And I searched neizvesten-peregovory-po-racii.mp3 data in the materials
	When I clicked on the first material in the Materials list
	And I set the session priority to Important
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Important
	When I set the session priority to Immediate Report
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Immediate Report
	When I set the session priority to Translation
	When I pressed the Next material button
	When I pressed the Previous material button
	Then I must see that the session priority value must be set to Translation