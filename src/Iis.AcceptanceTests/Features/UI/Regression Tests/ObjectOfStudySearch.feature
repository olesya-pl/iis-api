Feature: ObjectsOfStudySearch - functional
    - IIS-6140 - Search by two criteria by using OR operator
    - IIS-6139 - Search by two criteria by using NOT operator
    - IIS-6138 - Search by two criteria by using AND operator
    - IIS-6082 - Search object of study by full name
    - IIS-6207 - Open a small object of study card

Background:
	Given I sign in with the user olya and password 123 in the Contour

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-6140 - Search by two criteria by using OR operator
	When I clicked on search button in the Object of study section
	And I searched Олександр OR Іванович data in the Objects of study section
	Then I must see object of study ОТРОЩЕНКО Олександр Іванович as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-6139 - Search by two criteria by using NOT operator
	When I clicked on search button in the Object of study section
	And I searched Олександр NOT Іванович data in the Objects of study section
	Then I must not see object of study ОТРОЩЕНКО Олександр Іванович as first search result

@functional @UI @ObjectsOfStudySearchUI
Scenario: IIS-6138 - Search by two criteria by using AND operator
	When I clicked on search button in the Object of study section
	And I searched Ткачук AND "3 омсбр" data in the Objects of study section
	Then I must see object of study ТКАЧУК Руслан Юрійович as first search result
	Then I must see search results counter value that equal to 1 value

@functional @sanity @UI @ObjectsOfStudySearchUI
Scenario: IIS-6082 - Search object of study by full name
	When I clicked on search button in the Object of study section
	And I searched в/ч 85683-А data in the Objects of study section
	Then I must see object of study радіотехнічний батальойн в/ч 85683-А as first search result

@smoke @UI @ObjectOfStudySmallCardUI
Scenario: IIS-6207 - Open a small object of study card
	When I clicked on first object of study
	Then I must see the object of study small card

@UI @functional ObjectOfStudyCreation
Scenario: IIS-6127 - Fill all the fields in the military organization and save it
	When I clicked on the create a new object of study button
	And I clicked on the create a new military organization button
	And I filled in the form
		
		 | Accordion                                 | FieldName                           | FieldValueValue                             |
		 |                                           | Приналежність                       | ворожий                                     |
		 |                                           | Важливість                          | першочерговий                               |
		#| Безпосереднє підпорядкування              | Безпосереднє підпорядкування        | 1 бру                                       |
		#| Бойовий досвід                            | Бойовий досвід                      | тестове значення бойового досвіду           |
		#| Війскова частина                          | Війскова частина                    | тест номер ВЧ                               |
		#| Військовий гарнізон                       | Військовий гарнізон                 | Гарнізон перший                             |
		#| Дані щодо БТГр                            | Дані щодо БТГр                      | Тестові дані щодо БТГр                      |
		#| Дані щодо РТГр                            | Дані щодо РТГр                      | Тестові дані щодо РТГр                      |
		 | Дислокація                                | Країна                              | Болгарія                                    |
		#| Дислокація                                | Республіка/край/область             | Тестова назва республіки                    |
		#| Дислокація                                | Адміністративний район              | Тестовий адміністративний район             |
		#| Дислокація                                | Населений пункт                     | Тестовий населений пункт                    |
		#| Дислокація                                | Адреса поштова                      | Тестова адреса поштова                      |
		#| Дислокація                                | Які підрозділи дислокуються         | Тестові підрозділи, що дислокуються         |
		#| Дислокація                                | Найменування військового містечка   | Тестове найменування військового містечка   |
		#| Додаткова інформація                      | Додаткова інформація                | Тестове значення додаткової інформації      |
		#| Загальна інформація                       | Найменування дійсне повне розширене | Тестове найменування дійсне повне розширене |
		#| Загальна інформація                       | Найменування дійсне скорочене       | Тестове найменування дійсне скорочене       |
		#| Загальна інформація                       | Умовне (відкрите) найменування      | Тестове умовне (відкрите) найменування      |
		#| Загальна інформація                       | Оперативне (бойове) призначення     | Тестове оперативне (бойове) призначення     |
		#| Загальна інформація                       | Найменування дійсне повне           | Тестове найменування дійсне повне           |
		#| Засоби чергового зв                       | ЗС СПД                              | Тестове ЗС СПД                              |
		#| Засоби чергового зв                       | АТС-О                               | Тестове АТС-О                               |
		#| Засоби чергового зв                       | АТС-Р                               | Тестове АТС-Р                               |
		#| Класифікатори                             | Пряме підпорядкування               | Підрозділ без назви                         |
		#| Класифікатори                             | SIDC                                | UNIT                                        |
		#| Класифікатори                             | Вид збройних сил, рід військ        | Розвідки та РЕБ                             |
		#| Командний склад                           | Командир                            | Тестовий командир                           |
		#| Командний склад                           | Начальник штабу                     | Тестовий начальник штабу                    |
		#| Командний склад                           | Інші посадові особи                 | Тестові інші посадові особи                 |
		#| Коротка історична довідка                 | Коротка історична довідка           | Тестова коротка історична довідка           |
		 | Країна                                    | Країна                              | Китай                                       |
		#| Мобілізаційні питання                     | Мобілізаційні питання               | Тестові мобілізаційні питання               |
		#| Найближче місце завантаження на транспорт | Залізнична станція                  | Тестова залізнична станція                  |
		#| Найближче місце завантаження на транспорт | Платформа завантаження              | Тестова платформа завантаження              |
		#| Найближче місце завантаження на транспорт | Аеродром                            | Тестовий аеродром                           |
		#| Найближче місце завантаження на транспорт | Порт                                | Тестовий порт                               |
		#| Найближче місце завантаження на транспорт | Країна                              | Китай                                       |
		#| Найближче місце завантаження на транспорт | Республіка/край/область             | Тестова назва республіки                    |
