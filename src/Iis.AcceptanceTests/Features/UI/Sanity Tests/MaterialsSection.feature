Feature: MaterialsSectionUI - Sanity

    - IIS-6375 - Material processing, priority and importance setup
    - IIS-6374 - ML results display for DOCX material
    - IIS-5837- Connect a material with an object of study from material
    - IIS-6363 - Search a material by keyword from the material

    Background:
        Given I sign in with the user olya and password 123 in the Contour

    @sanity @UI @MaterialsSanityUI
    Scenario: IIS-6375 - Material processing, priority and importance setup
        When I navigated to Materials page
        And I clicked on the first material in the Materials list
        And I set importance Друга категорія value
        And I set reliability Достовірна value
        And I pressed Processed button
        When I pressed Back button
        Then I must see that importance value must be set to Друга категорія value
        Then I must see that reliability value must be set to Достовірна value

    @sanity @UI @MaterialsSanityUI
    Scenario: IIS-6374 - ML results display for DOCX material
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched 20201015_Resilience_Application_Form.docx data in the materials
        And I clicked on the first search result in the Materials section
        And I clicked on the ML tab in the material card
        And I pressed Show button to show Text classifier ML output
        Then I must see Text classifier ML output form

    @sanity @UI @MaterialsSanityUI
    Scenario: IIS-5837- Connect a material with an object of study from material
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched 20201015_Resilience_Application_Form.docx data in the materials
        And I clicked on the first search result in the Materials section
        And I clicked on the relations tab in the material card
        And I enter Романов value in the search object field
        When I clicked on the connected object
        Then I must see РОМАНОВ А.Г title of the object
        When I clicked Back button in the browser
        And I clicked on the relations tab in the material card
        And I clicked on the delete button to destroy relation between the material and the РОМАНОВ А.Г object
        When I pressed the confirm button
        Then I must not see the related РОМАНОВ А.Г object in the material

    @sanity @UI @MaterialsSanityUI
    Scenario: IIS-6363 - Search a material by keyword from the material
        When I navigated to Materials page
        And I clicked search button in the Materials section
        And I searched NATO data in the materials
        Then I must see a material that contains NATO word in the Materials search result






