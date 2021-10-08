Feature: SearchFunctionalityUI - Regression

	- IIS-6791 - Possibility to search an object of study by concrete name
    - IIS-6081 - Possibility to search an object of study by a field name
    - IIS-6055 - Possibility to search an object by a fragment of a description
    - IIS-5953 - Possibility to search a material by a creation date 

	Background:
		Given I sign in with the user olya and password 123 in the Contour

	@regression @UI @OOSSearchUI
	Scenario: IIS-6791 - Possibility to search an object of study by concrete name
		When I clicked on search button in the Object of study section
		And I entered the value БФ РФ in the search field
		Then I must see the БФ РФ value in the search suggestion list
		When I clicked on the БФ РФ autocomplete button
		Then I see the БФ РФ tag in the search field
		When I clicked on the first search result title in the Objects of study section
		Then I must see the title БФ РФ in the small card

    @regression @UI @OOSSearchUI
	Scenario: IIS-6081 - Possibility to search an object of study by a field name
		When I clicked on search button in the Object of study section
		And I searched commonInfo.OpenName:"в/ч 85683-А" data in the Objects of study section
		When I clicked on the first search result title in the Objects of study section
		Then I must see the title ртб in the small card

    @regression @UI @OOSSearchUI
	Scenario: IIS-6081 - Possibility to search an object by a fragment of a description
		When I clicked on the create a new object of study button
	    And I clicked on the create a new military organization button
	    When I filled in the form
		
		 | Accordion                    | FieldName                    | FieldValueValue                                                  |
		 |                              | Приналежність                | ворожий                                                          |
		 |                              | Важливість                   | першочерговий                                                    |
	     |                              | Гриф (рівень доступу)        | НВ - Не визначено                                                |
		 | Додаткова інформація         | Додаткова інформація         | Дотакова інформація, по частині тексту якої буде проведено пошук |
		Then I must see Підрозділ без назви title of the object
		When I clicked on the Objects section
		When I clicked on search button in the Object of study section
		And I searched additionalInfo:"Дотакова інформація, по частині тексту" data in the Objects of study section
		When I clicked on the first search result title in the Objects of study section
		Then I must see the title Підрозділ без назви in the small card

    @regression @UI @MaterialsSearchUI
	Scenario: IIS-5953 - Possibility to search a material by a creation date
		When I navigated to Materials page
		And I clicked search button in the Materials section
	    And I searched CreatedDate: 2021,09,10 AND Договір Костюми 2020.pdf data in the materials
		Then I must see the Договір Костюми 2020.pdf title of the material
		When I clicked on the clear search button
		And I clicked search button in the Materials section
	    And I searched CreatedDate: 2021,09,10 AND Договір Костюми 2020.pdf data in the materials
		Then I must see the Договір Костюми 2020.pdf title of the material
		When I clicked on the clear search button
		And I clicked search button in the Materials section
	    And I searched CreatedDate: 2021,09,10 AND Договір Костюми 2020.pdf data in the materials
		Then I must see the Договір Костюми 2020.pdf title of the material
		When I clicked on the clear search button
		And I clicked search button in the Materials section
	    And I searched CreatedDate: 2021,09,10 AND Договір Костюми 2020.pdf data in the materials
		Then I must see the Договір Костюми 2020.pdf title of the material
		
	

