Feature: EventsSection - sanity

    - IIS-6364 - Bind a material to an event in the event EventsSection

    Background: Authorize
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @EventsSectionSanity @UI
    Scenario: IIS-6364 - Bind a material to an event in the event EventsSection
        When I navigated to Events page
        And I clicked on the Захід ркр Варяг до ВМБ Севастополь event in the event list
        And I clicked on the Новое событе тестовое event in the event list


#Then the material must be binded to the event