Feature: MaterialsSectionUI - Smoke

    - Open Materials section

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @smoke @UI @MaterialsUI
    Scenario: Ensure that Materials section is opened
        When I navigated to Materials page
        Then I must see the Materials page
        Then I must see first user in the user list