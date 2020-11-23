Feature: LoadMaterialsSectionUI - Smoke

    - IIS-6329 - Ensure that Load materials section is opened

    Background:
        Given I sign in with the user olya and password 123 in the Contour


    @smoke @UI @LoadMaterialsSectionUI
    Scenario: IIS-6329 - Ensure that Load materials section is opened
        When I navigated to the Upload materials page
        Then I must see choose file for upload button in the Upload materials section