Feature: ObjectsOfStudySection - sanity

    - IIS-6119 - Possibility to switch between hierarchy objects in the OOS section
    - IIS-6210 - Ensure that search by using ! symbol gives 0 search results
    - IIS-6207 - Open a small object of study card
    - IIS-6208 - Open a big object of study card

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @UI @ObjectOfStudySectionUI
    Scenario: IIS-6119 - Possibility to switch between hierarchy objects in the OOS section
        When I clicked on Hierarchy tab in the Object of study section
        And I double clicked on Russian military forces expand button