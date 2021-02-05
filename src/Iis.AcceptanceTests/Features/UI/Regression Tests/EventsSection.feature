Feature: Events - regression

    - IIS-5877 - Create an event by filling in all the data
    - IIS-6051 - Ability to connect the material to an event from a material
    - IIS-5993 - Ability to loose connection between the material to an event from the event section
    - IIS-5992 - Ability to connect the event and a material


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
    Scenario: IIS-5993 - Ability to loose connection between the material to an event from the event section
        When I navigated to Events page
        And I created a new Тестова подія event
        When I searched for the Тестова подія created event
        Then I must see the Тестова подія event in the event search results
        When I pressed the edit event button
        And I binded a 257 material to the event
        When I pressed the save event changes button
        Then I must see the Voice_01-07-2017 21-34-38 (257) material binded to the event
        When I pressed the delete button to delete the specified Voice_01-07-2017 21-34-38 (257).mp3 material
        Then I must not see the Voice_01-07-2017 21-34-38 (257).mp3 material binded to the event

    @regression @UI @Events
    Scenario: IIS-5992 - Ability to connect the event and a material
        When I navigated to Events page
        And I created a new Тестова подія event
        When I searched for the Тестова подія created event
        Then I must see the Тестова подія event in the event search results
        When I pressed the edit event button
        And I binded a 257 material to the event
        When I pressed the save event changes button
        Then I must see the Voice_01-07-2017 21-34-38 (257) material binded to the event
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched Voice_01-07-2017 21-34-38 (257) data in the materials
        And I clicked on the first material in the Materials list
        When I clicked on the relations tab in the material card
        Then I must see Тестова подія as the related event to the material