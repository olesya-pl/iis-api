Feature: ThemesAndUpdatesSection - sanity

    - IIS-6408 - Create a theme based on an object of study

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @UI @ThemesAndUpdatesSectionSanityUI
    Scenario: IIS-6408 - Create a theme based on an object of study
        When I clicked on search button in the Object of study section
        And I enter Попов value in the search object field
        And I pressed on the Create theme button in the objects section
        And I entered the Тестова тема theme name in the objects section
        When I navigated to Themes and updates section
        Then I must see a theme with specified name
