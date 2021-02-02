Feature: Events - regression

    - IIS-5877 - Create an event by filling in all the data
    - IIS-6051 - Ability to connect the material to an event from a material
    - IIS-5993 - Ability to loose connection between the material to an event


    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @regression @UI @Events
    Scenario: IIS-5877 - Create an event by filling in all the data
        When I navigated to Events page
        And I created a new Тестова подія event by filling in all the data
        When I searched for the Тестова подія created event
        Then I must see the Тестова подія event in the event search results


    @regression @UI @Events
    Scenario: IIS-6051 - Ability to connect the material to an event from a material
        When I navigated to Events page
        And I created a new Тестова подія event
        When I searched for the Тестова подія created event
        Then I must see the Тестова подія event in the event search results
        When I pressed the edit event button
        And I binded a 257 material to the event
        When I pressed the save event changes button
        Then I must see the Voice_01-07-2017 21-34-38 (257) material binded to the event

    @regression @UI @Events
    Scenario: IIS-6051 - Ability to connect the material to an event from a material
        When I navigated to Events page
        And I created a new Тестова подія event
        When I searched for the Тестова подія created event
        Then I must see the Тестова подія event in the event search results
        When I pressed the edit event button
        And I binded a 257 material to the event
        When I pressed the save event changes button
        Then I must see the Voice_01-07-2017 21-34-38 (257) material binded to the event
        When I pre