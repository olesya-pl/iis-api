Feature: ReportSection - sanity

    - IIS-6408 - Create and view report

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @UI @ReportSectionSanityUI
    Scenario: IIS-6408 - Create and view report
        When I navigated to Report section
        And I pressed the Create a new report button
        And I entered the Тестовий отримувач recipient name
        And I pressed the Proceed button
        And I selected the first event from the event list
        And I pressed Add report button
        Then I must see an event in the report
        When I selected the first event from the report
        And I pressed Remove report button
        Then I must not see an event in the report
        And I selected the first event from the event list
        And I pressed Add report button
        And I pressed the Save button
        And I pressed the Confirm button
        Then I must see a report with specified name