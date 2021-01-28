Feature: Materials - regression

    - IIS-6109 - Indicate a phone number pattern of a cell voice material
    - IIS-6048 - Change a material priority by clicking on the Processed button
    - IIS-6045 - Change a material reliability by clicking on the Processed button

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @regression @UI @Materials
    Scenario: IIS-6109 - Indicate a phone number pattern of a cell voice material
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched Voice_01-07-2017 21-34-38 (257) data in the materials
        And I clicked on the first search result in the Materials section
        When I clicked on the pattern tab
        Then I must see that phone number pattern is equal to value

            """

            "380713066027"

            """

    @regression @UI @Materials
    Scenario: IIS-6048 - Change a material priority by clicking on the Processed button
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched Voice_01-07-2017 21-34-38 (257) data in the materials
        And I clicked on the first search result in the Materials section
        And I set the session priority Важливий value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the session priority value must be set to the Важливий value
        When I set the session priority Негайна доповідь value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the session priority value must be set to the Негайна доповідь value
        When I set the session priority Пропустити value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the session priority value must be set to the Пропустити value
        When I set the session priority Переклад value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the session priority value must be set to the Переклад value


    @regression @UI @Materials
    Scenario: IIS-6045 - Change a material reliability by clicking on the Processed button
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched Voice_01-07-2017 21-34-38 (257) data in the materials
        And I clicked on the first search result in the Materials section
        And I set the source credibility Повністю надійне value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the source credibility value must be set to the Повністю надійне value
        #When I set the source credibility Здебільшего надійне value
        #And I pressed Processed button
        #When I pressed the Previous material button
        #Then I must see that the source credibility value must be set to the Здебільшего надійне value
        When I set the source credibility Відносно надійне value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the source credibility value must be set to the Відносно надійне value
        When I set the source credibility Не завжди надійне value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the source credibility value must be set to the Не завжди надійне value
        When I set the source credibility Ненадійне value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the source credibility value must be set to the Ненадійне value
        When I set the source credibility Неможливо оцінити надійність value
        And I pressed Processed button
        When I pressed the Previous material button
        Then I must see that the source credibility value must be set to the Неможливо оцінити надійність value