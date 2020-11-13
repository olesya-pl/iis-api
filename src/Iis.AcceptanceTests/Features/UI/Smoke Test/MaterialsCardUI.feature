Feature: MaterialsCardUI - Smoke

    - Open the material card
    - Open events tab
    - Open general tab
    - Open objects tab

    Background:
        Given I sign in with the user olya and password 123 in the Contour


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

            | ImportanceDropDown         |
            | RelevanceDropDown          |
            | СompletenessOfInformation |
            | SourceCredibility          |
            | Originator                 |

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



