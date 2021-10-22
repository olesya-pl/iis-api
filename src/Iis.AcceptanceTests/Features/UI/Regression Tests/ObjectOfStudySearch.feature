Feature: ObjectsOfStudySearch - functional
    - IIS-6140 - Search by two criteria by using OR operator
    - IIS-6139 - Search by two criteria by using NOT operator
    - IIS-6138 - Search by two criteria by using AND operator
    - IIS-6082 - Search object of study by full name
    - IIS-6207 - Open a small object of study card
	- IIS-5830 - Search object by mobile phone sign
	- IIS-0007 - Search militaryRank on object page

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
	And I searched в/ч 85683-А військовий підрозділ data in the Objects of study section
	Then I must see object of study радіотехнічний батальойн в/ч 85683-А as first search result

@smoke @UI @ObjectOfStudySmallCardUI
Scenario: IIS-6207 - Open a small object of study card
	When I clicked on first object of study
	Then I must see the object of study small card

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
Scenario: IIS-0011 - Search on object page1
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched Бонд data in the Objects of study section
	Then I must see object of study Бонд Д. as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0002 - Search on object page2
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see object of study полковник as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0003 - Search on object page3
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched __title: "ГУСЄВ С." AND militaryRank:полковник data in the Objects of study section 
	Then I must see object of study Звання: полковник, __title: ГУСЄВ С., as first search result
	
@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0004 - Search on object page4
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see object of study полковник as first search result
	
@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0005 - Search on object page5
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see object of study полковник as first search result
	
@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0006 - Search on object page6
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see object of study полковник as first search result
	
@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0007 - Search militaryRank on object page
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see military ranks Звання: полковник, by objects of study in result
	
@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0008 - Search on object page8
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see object of study полковник as first search result
	
@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0009 - Search on object page9
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see object of study полковник as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-0010 - Search on object page10
	When I clicked on the Objects section
	And I clicked on search button in the Object of study section
	And I searched militaryRank:полковник data in the Objects of study section
	Then I must see object of study полковник as first search result