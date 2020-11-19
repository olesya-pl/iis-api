Feature: ObjectsOfStudySearch - smoke

    - IIS-6207 - Open a small object of study card

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @smoke @UI @ObjectOfStudySmallCardUI
    Scenario: IIS-6207 - Open a small object of study card
        When I clicked on first object of study
        Then I must see the object of study small card
