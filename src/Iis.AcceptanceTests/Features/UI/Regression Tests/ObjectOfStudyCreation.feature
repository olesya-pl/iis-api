﻿Feature: ObjectOfStudyCreation - functional
         - IIS-6127 - - Fill all the fields in the military organization and create it

Background:
	Given I sign in with the user olya and password 123 in the Contour

@UI @functional ObjectOfStudyCreation
Scenario: IIS-6127 - Fill all the fields in the military organization and create it
	When I clicked on the create a new object of study button
	And I clicked on the create a new military organization button
	When I filled in the form
		
		 | Accordion                    | FieldName                    | FieldValueValue                   |
		 |                              | Приналежність                | ворожий                           |
		 |                              | Важливість                   | першочерговий                     |
	     |                              | Гриф (рівень доступу)        | НВ - Не визначено                 |
		 | Безпосереднє підпорядкування | Безпосереднє підпорядкування | 1 бру                             |
		 | Бойовий досвід               | Бойовий досвід               | тестове значення бойового досвіду |
		 | Війскова частина             | Війскова частина             | тест номер ВЧ                     |
		 | Військовий гарнізон          | Військовий гарнізон          | Гарнізон перший                   |
		 | Дані щодо БТГр                                 | Дані щодо БТГр                                                  | Тестові дані щодо БТГр                                                 |
		 | Дані щодо РТГр                                 | Дані щодо РТГр                                                  | Тестові дані щодо РТГр                                                 |
		 | Дислокація                                     | Країна                                                          | Болгарія                                                               |
		 | Дислокація                                     | Республіка/край/область                                         | Тестова назва республіки                                               |
		 | Дислокація                                     | Адміністративний район                                          | Тестовий адміністративний район                                        |
		 | Дислокація                                     | Населений пункт                                                 | Тестовий населений пункт                                               |
		 | Дислокація                                     | Адреса поштова                                                  | Тестова адреса поштова                                                 |
		 | Дислокація                                     | Які підрозділи дислокуються                                     | Тестові підрозділи, що дислокуються                                    |
		 | Дислокація                                     | Найменування військового містечка                               | Тестове найменування військового містечка                              |
		 | Додаткова інформація                           | Додаткова інформація                                            | Тестове значення додаткової інформації                                 |
		 | Загальна інформація                            | Найменування дійсне повне розширене                             | Тестове найменування дійсне повне розширене                            |
		 | Загальна інформація                            | Найменування дійсне скорочене                                   | Тестове найменування дійсне скорочене                                  |
		 | Загальна інформація                            | Умовне (відкрите) найменування                                  | Тестове умовне (відкрите) найменування                                 |
		 | Загальна інформація                            | Оперативне (бойове) призначення                                 | Тестове оперативне (бойове) призначення                                |
		 #| Загальна інформація                            | Найменування дійсне повне                                       | Тестове найменування дійсне повне                                      |
		 | Засоби чергового зв'язку                       | ЗС СПД                                                          | Тестове ЗС СПД                                                         |
		 | Засоби чергового зв'язку                       | АТС-О                                                           | Тестове АТС-О                                                          |
		 | Засоби чергового зв'язку                       | АТС-Р                                                           | Тестове АТС-Р                                                          |
		 | Класифікатори                                  | Пряме підпорядкування                                           | Підрозділ без назви                                                    |
		 | Класифікатори                                  | SIDC                                                            | UNIT                                                                   |
		 | Класифікатори                                  | Вид збройних сил, рід військ                                    | Розвідки та РЕБ                                                        |
		 | Командний склад                                | Командир                                                        | Тестовий командир                                                      |
		 | Командний склад                                | Начальник штабу                                                 | Тестовий начальник штабу                                               |
		 | Командний склад                                | Інші посадові особи                                             | Тестові інші посадові особи                                            |
		 | Коротка історична довідка                      | Коротка історична довідка                                       | Тестова коротка історична довідка                                      |
		 | Країна                                         | Країна                                                          | Китай                                                                  |
		 | Мобілізаційні питання                          | Мобілізаційне завдання                                          | Тестові мобілізаційні питання                                          |
		 | Найближче місце завантаження на транспорт      | Залізнична станція                                              | Тестова залізнична станція                                             |
		 | Найближче місце завантаження на транспорт      | Платформа завантаження                                          | Тестова платформа завантаження                                         |
		 | Найближче місце завантаження на транспорт      | Аеродром                                                        | Тестовий аеродром                                                      |
		 | Найближче місце завантаження на транспорт      | Порт                                                            | Тестовий порт                                                          |
		 | Найближче місце завантаження на транспорт      | Країна                                                          | Китай                                                                  |
		 | Найближче місце завантаження на транспорт      | Республіка/край/область                                         | Тестова назва республіки                                               |
		 | Найближче місце завантаження на транспорт      | Адміністративний район                                          | Тестовий адміністративний район                                        |
		 | Найближче місце завантаження на транспорт      | Населений пункт                                                 | Тестовий населений пункт                                               |
		 | Найближче місце завантаження на транспорт      | Адреса поштова                                                  | Тестова адреса поштова                                                 |
		 | Номер штату                                    | Номер штату                                                     | Тестовий номер штату                                                   |
		 | Номерні знаки автомобільної техніки            | Номерні знаки автомобільної техніки                             | Тестові номерні знаки автомобільної техніки                            |
		 | Оперативна, бойова та мобілізаційна підготовка | Оперативна, бойова та мобілізаційна підготовка                  | Тестова оперативна, бойова та мобілізаційна підготовка                 |
		 | Оперативна, бойова та мобілізаційна підготовка | Основні заходи бойової підготовки                               | Тестові основні заходи бойової підготовки                              |
		 | Оперативна, бойова та мобілізаційна підготовка | Основні заходи оперативної підготовки                           | Тестові основні заходи оперативної підготовки                          |
		 | Організаційно-штатна структура                 | Опис                                                            | Тестовий опис                                                          |
		 | Організаційно-штатна структура                 | Заплановані організаційно-штатні заходи                         | Тестові заплановані організаційно-штатні заходи                        |
		 | Останнє підтвердження                          | Останнє підтвердження                                           | 2021-03-29                                                             |
		 | Полігони                                       | Періодично використовуються                                     | Тестово періодично використовуються                                    |
		 | Полігони                                       | Власні полігони                                                 | Тестові власні полігони                                                |
		 | Тимчасова дислокація                           | Країна                                                          | Китай                                                                  |
		 | Тимчасова дислокація                           | Республіка/край/область                                         | Тестова назва республіки                                               |
		 | Тимчасова дислокація                           | Адміністративний район                                          | Тестовий адміністративний район                                        |
		 | Тимчасова дислокація                           | Населений пункт                                                 | Тестовий населений пункт                                               |
		 | Тимчасова дислокація                           | Адреса поштова                                                  | Тестова адреса поштова                                                 |
		 | Чисельність                                    | Чисельність                                                     | Взвод                                                                  |
		 | Юрідичні дані                                  | Повне найменування юридичної особи зі свідоцтва про реєстрацію: | Тестове повне найменування юридичної особи зі свідоцтва про реєстрацію |
		 | Юрідичні дані                                  | ИНН (ідентифікаційний номер платника податків)                  | Тестовий ІНН (ідентифікаційний номер платника податків)                |
		 | Юрідичні дані                                  | ОДРН (основний державний реєстраційний номер)                   | Тестовий ОДРН (основний державний реєстраційний номер)                 |
		 | Юрідичні дані                                  | Адреса отримання почтових (нетаємних) і таємних відправлень     | Тестова адреса отримання почтових (нетаємних) і таємних відправлень    |
		 | Ідентифікаційні ознаки                         | Позивний вузлу зв'язку                                                         | Тестовий позивний вузлу зв'язку                                                        |
		 | Інформація щодо особового складу               | Загальна інформація щодо особового складу                                      | Тестова загальна інформація щодо особового складу                                      |
		 | Інформація щодо особового складу               | Додаткова інформація щодо чисельності о/с                                      | Тестова додаткова інформація щодо чисельності о/с                                      |
		 | Інформація щодо особового складу               | Навчальні центри підготовки особового складу за спеціальностями                | Тестові Навчальні центри підготовки особового складу за спеціальностями                |
		 | Інформація щодо особового складу               | Загальна характеристика особового складу та його морально-психологічного стану | Тестова загальна характеристика особового складу та його морально-психологічного стану |
			
	Then I must see Тестове найменування дійсне скорочене  title of the object

