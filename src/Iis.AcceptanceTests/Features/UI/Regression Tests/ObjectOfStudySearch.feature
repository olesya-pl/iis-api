Feature: ObjectsOfStudySearch - functional
	- IIS-6140 - Search by two criteria by using OR operator
	- IIS-6139 - Search by two criteria by using NOT operator
	- IIS-6138 - Search by two criteria by using AND operator
	- IIS-6082 - Search object of study by full name
	- IIS-5830 - Search object by mobile phone sign
	- IIS-0007 - Search militaryRank on object page
	- IIS-6791 - Possibility to search an object of study by concrete name
	- IIS-6081 - Possibility to search an object of study by a field name
	- IIS-6055 - Possibility to search an object by a fragment of a description

Background:
	Given I sign in with the user olya and password 123 in the Contour

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-6140 - Search by two criteria by using OR operator
	When I clicked on search button in the Object of study section
	And I searched Олександр OR Іванович data in the Objects of study section
	Then I must see object of study ОТРОЩЕНКО О.І as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-6139 - Search by two criteria by using NOT operator
	When I clicked on search button in the Object of study section
	And I searched Олександр NOT Іванович data in the Objects of study section
	Then I must not see object of study ОТРОЩЕНКО Олександр Іванович as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-6138 - Search by two criteria by using AND operator
	When I clicked on search button in the Object of study section
	And I searched Ткачук AND "3 омсбр" data in the Objects of study section
	Then I must see object of study ТКАЧУК Р.Ю as first search result
	Then I must see search results counter value that equal to 1 value

@functional @sanity @UI @ObjectsOfStudySearchUI
Scenario: IIS-6082 - Search object of study by full name
	When I clicked on search button in the Object of study section
	And I searched "в/ч 85683-А" data in the Objects of study section
	Then I must see object of study радіотехнічний батальойн в/ч 85683-А as first search result


@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-5830 - Search object by mobile phone sign
	When I clicked on search button in the Object of study section
	And I searched 0997908973 data in the Objects of study section
	Then I must see object of study Бонд as first search result
	Then I must see sign value 0997908973 in first search result

	@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0001 - Search on object page1
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched Бонд data in the Objects of study section
	Then I must see object of study Бонд Д. as first search result


	#Search

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0003 - Search on object page3
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched __title: "ГУСЄВ С." AND militaryRank:полковник data in the Objects of study section
	Then I must see object of study Звання: полковник, __title: ГУСЄВ С., as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0007 - Search militaryRank on object page
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see military ranks Звання: полковник, by objects of study in result

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
	And I searched commonInfo.OpenName: "в/ч 08801" data in the Objects of study section
	When I clicked on the first search result title in the Objects of study section
	Then I must see the title 1 омсбр in the small card

@regression @UI @OOSSearchUI
Scenario: IIS-6055 - Possibility to search an object by a fragment of a description
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