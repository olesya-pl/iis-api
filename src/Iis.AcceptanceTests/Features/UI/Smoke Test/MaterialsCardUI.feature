Feature: MaterialsCardUI - Smoke

    - Open the material card
    - Open events tab
    - Open general tab

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @smoke @UI @MaterialsCardUI
    Scenario: Ensure that the material card can be opened
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        Then I must see processed button in the materials card
        Then I must see relevance drop down in the materials card

    @smoke @UI @MaterialsCardEventsTabUI
    Scenario: Open events tab relation in the materials card
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        And I clicked on the events tab in the material card
        Then I must see events search in the materials card

    @smoke @UI @MaterialsCardGeneralTabUI
    Scenario: Open general tab in the materials card
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        #Then I must see these elements

        #| ImportanceDropDown         |
        #| RelevanceDropDown          |
        #| СompletenessOfInformation |
        #| SourceCredibility          |
        #| Originator                 |

        Then I must I must see at least one user in the originator drop down menu





