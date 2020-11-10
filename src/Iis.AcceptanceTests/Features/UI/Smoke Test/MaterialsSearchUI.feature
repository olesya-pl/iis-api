Feature: MaterialsSearchUI - Zero Results - Smoke

    - Search by using ! symbol in Materials section. Results should be equal to 0.

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @smoke @UI @MaterialsSearchUI
    Scenario: Ensure that search by using ! symbol gives 0 search results
        When I navigated to Materials page
        And I clicked Search button
        And I entered ! data in the search field
        Then I must see zero results