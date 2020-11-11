Feature: Entityes

Background: 
	Given Authorized user with login olya and password 123
	Given fetch required dictionaties

@mytag
Scenario Outline: User can create EntityRadionetwork
	And I have fullfield <AffiliationField>, <ImportanceField>, <NameField>, <TitleField>, <PurposeField>, <DescriptionField>, <LastConfirmedAt> in the object form	
	When I press add entity
	Then the entity creation result should have id and name should start with <NameField>
	Then entity creation request should be executed without errors
	Examples: 
	| AffiliationField | ImportanceField | NameField | TitleField                      | PurposeField   | DescriptionField | LastConfirmedAt          |
	| ворожий          | першочерговий   | APCO 25   | ПОРТАТИВНА РАДІОСТАНЦІЯ SRX2200 | Глушення ефіру | Колір Чорний     | 2020-07-16T21:00:00.000Z |
