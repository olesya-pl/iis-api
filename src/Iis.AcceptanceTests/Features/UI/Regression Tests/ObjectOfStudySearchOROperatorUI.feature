Feature: ObjectsOfStudySearch - functional

    - IIS-6140 - Search by two criteria by using OR operator
    - IIS-6139 - Search by two criteria by using NOT operator
    - IIS-6138 - Search by two criteria by using AND operator
    - IIS-6082 - Search object of study by full name

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @functional @UI @ObjectsOfStudySearchUI
    Scenario: IIS-6140 - Search by two criteria by using OR operator
        When I clicked on search button
        And I searched Олександр OR Іванович data in the objects section
        Then I must see object of study ОТРОЩЕНКО Олександр Іванович as first search result

    @functional @UI @ObjectsOfStudySearchUI
    Scenario: IIS-6139 - Search by two criteria by using NOT operator
        When I clicked on search button
        And I searched Олександр NOT Іванович data in the objects section
        Then I must not see object of study ОТРОЩЕНКО Олександр Іванович as first search result

    @functional @UI @ObjectsOfStudySearchUI
    Scenario: IIS-6138 - Search by two criteria by using AND operator
        When I clicked on search button
        And I searched Ткачук AND "3 омсбр" data in the objects section
        Then I must see object of study ТКАЧУК Руслан Юрійович as first search result
        Then I must see search results counter value that equal to 1 value

    @functional @UI @ObjectsOfStudySearchUI
    Scenario: IIS-6082 - Search object of study by full name
        When I clicked on search button
        And I searched в/ч 85683-А data in the objects section
        Then I must see object of study радіотехнічний батальойн в/ч 85683-А as first search result
