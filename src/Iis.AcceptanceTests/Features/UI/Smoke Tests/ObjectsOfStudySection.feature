Feature: ObjectsOfStudySection - smoke

    - IIS-6209 - Ensure that search by using * symbol gives all possible search results
    - IIS-6210 - Ensure that search by using ! symbol gives 0 search results
    - IIS-6207 - Open a small object of study card
    - IIS-6208 - Open a big object of study card

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @smoke @UI @ObjectOfStudySearchUI
    Scenario: IIS-6209 - Ensure that search by using * symbol gives all possible search results
        When I clicked on search button in the Object of study section
        And I got search counter value in the Object of study section
        And I searched * data in the Objects of study section
        Then I must see that search counter values are equal in the Objects of study section

    @smoke @UI @ObjectOfStudySearchUI
    Scenario: IIS-6210 - Ensure that search by using ! symbol gives 0 search results
        When I clicked on search button in the Object of study section
        And I searched ! data in the Objects of study section
        Then I must see zero search results in the Object of study page

    @smoke @sanity @UI @ObjectOfStudySmallCardUI
    Scenario: IIS-6207 - Open a small object of study card
        When I clicked on first object of study
        Then I must see the object of study small card
        Then I must see the title тест графа наземного базування in the small card

    @smoke @sanity @UI @ObjectOfStudySmallCardUI
    Scenario: IIS-6208 - Open a big object of study card
        When I clicked on first object of study
        And I clicked on enlarge small card button
        Then I must see these tabs in the big object of study card

            | BigCardProfileTab       |
            | BigCardMaterialsTab     |
            | BigCardEventsTab        |
            | BigCardChangeHistoryTab |
            | BigCardRelationsTab     |

        Then I must see the specific text blocks in big object of study card

            | BigCardAffiliation |
            | BigCardImportance  |



