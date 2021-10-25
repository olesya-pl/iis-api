Feature: EventsSection - sanity

    - IIS-6364 - Bind a material to an event in the event section
    - IIS-5831 - Bind an object of study to an event in the event section
    - IIS-6158 - Create an event in the event section

    Background: Authorize
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @EventsSectionSanity @UI
    Scenario: IIS-6364 - Bind a material to an event in the event section
        Given I upload a new docx material via API
		| Field                 | Value                                      |
		| FileName              | тестовий матеріал                          |
		| SourceReliabilityText | Здебільшого надійне                        |
		| ReliabilityText       | Достовірна                                 |
		| Content               | таємний контент                           |
		| AccessLevel           | 0                                          |
		| LoadedBy              | автотест                                   |
		| MetaData              | {"type":"document","source":"contour.doc"} |
        When I navigated to Events page
        And I created a new Тестова подія event
        And I searched for the Тестова подія created event
        And I pressed the review event button
        And I pressed the edit event button
        And I binded a  таємн  material to the event
        And I pressed the save event changes button
        When I reloaded the event page
        Then I must see the  таємн  material binded to the event

    @sanity @EventsSectionSanity @UI
    Scenario: IIS-5831 - Bind an object of study to an event in the event section
        When I navigated to Events page
        And I created a new Тестова подія event
        And I searched for the Тестова подія created event
        And I pressed the review event button
        And I pressed the edit event button
        And I binded an Попов object of study to the event
        And I pressed the save event changes button
        And I pressed the confirm save changes in the event
        And I reloaded the event page
        And I clicked on the Попов binded object of study in the event
        And I clicked on enlarge small card button
        When I navigated to the Events tab in the big object of study card
        Then I must see that the Тестова подія event related to the object of study

    @sanity @EventsSectionSanity @UI
    Scenario: IIS-6158 - Create an event in the event section
        When I navigated to Events page
        And I created a new Тестова подія event
        When I searched for the Тестова подія created event
        Then I must see the Тестова подія event in the event search results
