Feature: MaterialsSectionUI - Smoke

    - IIS-6187 - Ensure that Materials section is opened
    - IIS-6205 - Ensure that search by using ! symbol gives 0 search results
    - IIS-6204 - Ensure that search by using * symbol gives all possible search results
    - IIS-6188 - Ensure that the material card can be opened
    - IIS-6192 - Open events tab relation in the materials card
    - IIS-6189 - Open general tab in the materials card
    - IIS-6191 - Open relations tab in the materials card and ensure that objects search is present
    - IIS-6190 - Open ML tab in the materials card

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @smoke @UI @MaterialsUI
    Scenario: IIS-6187 - Ensure that Materials section is opened
        When I navigated to Materials page
        And Loading icon is not displayed
        Then I must see the Materials page
        Then I must see first user in the user list

    @smoke @UI @MaterialsSearchUI
    Scenario: IIS-6205 - Ensure that search by using ! symbol gives 0 search results
        When I navigated to Materials page
        And Loading icon is not displayed
        And I clicked search button in the Materials section
        And I searched ! data in the materials
        And Loading icon is not displayed
        Then I must see zero results in the Materials section

    @smoke @UI @MaterialsSearchUI
    Scenario: IIS-6204 - Ensure that search by using * symbol gives all possible search results
        When I navigated to Materials page
        And Loading icon is not displayed
        And I clicked search button in the Materials section
        And I got search counter value in the Materials section
        And I searched * data in the materials
        And Loading icon is not displayed
        Then I must see that search counter values are equal in the Materials section

    @smoke @UI @MaterialsCardUI
    Scenario: IIS-6188 - Ensure that the material card can be opened
    Given I upload a new docx material via API
		| Field                 | Value                                      |
		| FileName              | тестовий матеріал                          |
		| SourceReliabilityText | Здебільшого надійне                        |
		| ReliabilityText       | Достовірна                                 |
		| Content               | Просто контент                            |
		| AccessLevel           | 0                                          |
		| LoadedBy              | автотест                                   |
		| MetaData              | {"type":"document","source":"contour.doc"} |
        When I navigated to Materials page
        And Loading icon is not displayed
        And I clicked search button in the Materials section
        And I searched Просто data in the materials
        And Loading icon is not displayed
        And I clicked on the first material in the Materials list
        Then I must see relevance drop down in the materials card
        When I clean up uploaded material via API

    @smoke @UI @MaterialsCardEventsTabUI
    Scenario: IIS-6192 - Open relations tab in the materials card and ensure that events search is present
    Given I upload a new docx material via API
		| Field                 | Value                                      |
		| FileName              | тестовий матеріал                          |
		| SourceReliabilityText | Здебільшого надійне                        |
		| ReliabilityText       | Достовірна                                 |
		| Content               | Просто контент                            |
		| AccessLevel           | 0                                          |
		| LoadedBy              | автотест                                   |
		| MetaData              | {"type":"document","source":"contour.doc"} |
        When I navigated to Materials page
        And Loading icon is not displayed
        And I clicked search button in the Materials section        
        And I searched Просто data in the materials
        And Loading icon is not displayed
        And I clicked on the first material in the Materials list
        And I clicked on the relations tab in the material card
        Then I must see events search in the materials card
        When I clean up uploaded material via API

    @smoke @UI @MaterialsCardGeneralTabUI
    Scenario: IIS-6189 - Open general tab in the materials card
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
        And Loading icon is not displayed
        And I clicked on the first material in the Materials list
        Then I must see these elements

            | ImportanceDropDown        |
            | RelevanceDropDown         |
            | СompletenessOfInformation |
            | SourceCredibility         |
            | Originator                |

        Then I must I must see at least one user in the originator drop down menu
        When I clean up uploaded material via API

    @smoke @UI @MaterialsCardGeneralTabUI
    Scenario: IIS-6191 - Open relations tab in the materials card and ensure that objects search is present
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
        And Loading icon is not displayed
        And I clicked search button in the Materials section
        And I searched таємний data in the materials
        And Loading icon is not displayed
        And I clicked on the first material in the Materials list
        And I clicked on the relations tab in the material card
        Then I must see objects search in the materials card
        When I clean up uploaded material via API

    @smoke @UI @MaterialsCardGeneralTabUI
    Scenario: IIS-6190 - Open ML tab in the materials card
        When I navigated to Materials page
        And Loading icon is not displayed
        And I clicked search button in the Materials section
        And I searched таємний data in the materials
        And Loading icon is not displayed
        And I clicked on the first material in the Materials list
        And I clicked on the ML tab in the material card
        Then I must see Show button in the ML tab