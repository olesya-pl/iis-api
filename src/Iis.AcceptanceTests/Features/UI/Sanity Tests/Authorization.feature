Feature: AuthorizationSection - Sanity

    - IIS-5797 - User can log out

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @UI @AuthorizationSanityUI
    Scenario: IIS-5797 - User can log out
        When I pressed Sign out button
        And I confirmed the log out operation
        Then I must see the Contour main page