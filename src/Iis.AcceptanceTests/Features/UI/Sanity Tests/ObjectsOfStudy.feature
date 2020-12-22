Feature: ObjectsOfStudySection - sanity

    - IIS-6119 - Possibility to switch between hierarchy objects in the OOS section
    - IIS-6211 - Search results must contain a specific result
    - IIS-6370 - View and interact with data in profile in the objects section
    - IIS-5885 - Create a military organization

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @UI @ObjectOfStudySectionUI
    Scenario: IIS-6119 - Possibility to switch between hierarchy objects in the OOS section
        When I clicked on Hierarchy tab in the Object of study section
        And I double clicked on the Силові структури card in the hierarchy
        Then I must see these cards in hierarchy

            | ФСБ РФ   |
            | СЗР РФ   |
            | ФСВНГ РФ |
            | ЗС РФ    |

        When I double clicked on the ЗС РФ expand button in the hierarchy
        Then I must see these cards in hierarchy

            | ЗВО                                      |
            | ГШ ЗС РФ                                 |
            | Центральні органи військового управління |
            | ОСК Північ                               |


    @sanity @UI @ObjectOfStudySectionUI
    Scenario: IIS-6211 - Search results must contain third brigade Berkut
        When I clicked on search button in the Object of study section
        And I searched 3 омсбр data in the Objects of study section
        Then I must see the specified result

            | 3 окрема мотострілецька бригада "Беркут" |


    @sanity @UI @ObjectOfStudySectionUI
    Scenario: IIS-6370 - View and interact with data in profile in the objects section
        When I clicked on search button in the Object of study section
        And I searched 3 омсбр data in the Objects of study section
        And I clicked on the first search result title in the Objects of study section
        And I clicked on enlarge small card button
        And I clicked on the Classifier block in the big card window
        And I clicked on the Direct reporting relationship link in the big card window
        Then I must see the title 1 армійський корпус in the small card
        When I clicked on the General info block in the big card window
        Then I must see name real full is equal to the 3 окрема мотострілецька бригада Беркут value

    @sanity @UI @ObjectOfStudySectionUI
    Scenario: IS-5885 - Create a military organization
        When I clicked on the create a new object of study button
        And I clicked on the create a new military organization button
        And I entered the джокер value in the affiliation field
        And I entered the першочерговий value in the importance field
        And I clicked on the classifiers block
        And I entered the Балтійський флот value in the direct reporting relationship field




