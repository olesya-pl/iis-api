Feature: ObjectsOfStudySection - sanity

    - IIS-6119 - Possibility to switch between hierarchy objects in the OOS section
    - IIS-6211 - Search results must contain a specific result
    - IIS-6207 - Open a small object of study card
    - IIS-6208 - Open a big object of study card

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
        Then I must see third brigade Berkut as one of the search results



