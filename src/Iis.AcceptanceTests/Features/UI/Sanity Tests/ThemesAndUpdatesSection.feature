Feature: ThemesAndUpdatesSection - sanity

    - IIS-6408 - Create a theme based on an object of study

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @UI @ThemesAndUpdatesSectionSanityUI
    Scenario: IIS-6408 - Create a theme based on an object of study
        When I clicked on search button in the Object of study section
        And I searched Попов data in the Objects of study section
        And I pressed on the Create theme button in the objects section
        And I entered the Тестова тема theme name in the objects section
        When I navigated to Themes and updates section
        Then I must see a theme with specified name

    @UI @ThemesAndUpdatesSectionUI
    Scenario: IIS-6159 - Delete theme
        Given I created a theme with a name Тестова тема
        When I navigated to Themes and updates section
        And I Delete theme Тестова тема
        Then I must not see a theme Тестова тема
