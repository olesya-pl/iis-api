Feature: MaterialsSectionUI - Sanity

    - IIS-6375 - Material processing, priority and importance setup
    - IIS-6205 - Ensure that search by using ! symbol gives 0 search results
    - IIS-6204 - Ensure that search by using * symbol gives all possible search results
    - IIS-6188 - Ensure that the material card can be opened
    - IIS-6192 - Open events tab relation in the materials card
    - IIS-6189 - Open general tab in the materials card
    - IIS-6191 - Open objects tab in the materials card
    - IIS-6190 - Open ML tab in the materials card

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @sanity @UI @MaterialsSanityUI
    Scenario: IS-6375 - Material processing, priority and importance setup
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        And I set importance Друга категорія value
        And I set relevance Посередня value
        And I pressed Processed button
        When I pressed Back button
        Then I must see that importance value must be set to Важливий value
        Then I must see that importance value must be set to Сумнівна value

