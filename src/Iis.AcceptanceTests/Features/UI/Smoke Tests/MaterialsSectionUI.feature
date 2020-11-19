Feature: MaterialsSectionUI - Smoke

    - IIS-6187 - Ensure that Materials section is opened
    - IIS-6205 - Ensure that search by using ! symbol gives 0 search results
    - IIS-6204 - Ensure that search by using * symbol gives all possible search results
    - IIS-6188 - Ensure that the material card can be opened
    - IIS-6192 - Open events tab relation in the materials card
    - IIS-6189 - Open general tab in the materials card
    - IIS-6191 - Open objects tab in the materials card
    - IIS-6190 - Open ML tab in the materials card

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @smoke @UI @MaterialsUI
    Scenario: IIS-6187 - Ensure that Materials section is opened
        When I navigated to Materials page
        Then I must see the Materials page
        Then I must see first user in the user list

    @smoke @UI @MaterialsSearchUI
    Scenario: IIS-6205 - Ensure that search by using ! symbol gives 0 search results
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched ! data in the materials
        Then I must see zero results in the Materials section

    @smoke @UI @MaterialsSearchUI
    Scenario: IIS-6204 - Ensure that search by using * symbol gives all possible search results
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I got search counter value in the Materials section
        And I searched * data in the materials
        Then I must see that search counter values are equal in the Materials section

    @smoke @UI @MaterialsCardUI
    Scenario: IIS-6188 - Ensure that the material card can be opened
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        Then I must see processed button in the materials card
        Then I must see relevance drop down in the materials card

    @smoke @UI @MaterialsCardEventsTabUI
    Scenario: IIS-6192 - Open events tab relation in the materials card
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        And I clicked on the events tab in the material card
        Then I must see events search in the materials card

    @smoke @UI @MaterialsCardGeneralTabUI
    Scenario: IIS-6189 - Open general tab in the materials card
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        Then I must see these elements

            | ImportanceDropDown        |
            | RelevanceDropDown         |
            | СompletenessOfInformation |
            | SourceCredibility         |
            | Originator                |

        Then I must I must see at least one user in the originator drop down menu

    @smoke @UI @MaterialsCardGeneralTabUI
    Scenario: IIS-6191 - Open objects tab in the materials card
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        And I clicked on the objects tab in the material card
        Then I must see objects search in the materials card

    @smoke @UI @MaterialsCardGeneralTabUI
    Scenario: IIS-6190 - Open ML tab in the materials card
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        And I clicked on the ML tab in the material card
        Then I must see Show button in the ML tab