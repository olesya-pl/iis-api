Feature: Events

Background: 
	Given Authorized user with login olya and password hammer69
	Given fetch required dictionaties

@mytag
Scenario Outline: User can create event with the object
	And I have fullfield <Name>, <State>, <ComponentField>, <ImportanceField>, <EventType>, <RelatesToCountry> in the event form	
	When I press add
	Then the result should have id and name should start with <Name>
	Then create request should be executed without errors
	Examples: 
	| Name          | State   | ComponentField | ImportanceField | EventType    | RelatesToCountry |
	| тестова подія | Активна | Тероризм       | Важливе         | Візити: інше | Росія            |
