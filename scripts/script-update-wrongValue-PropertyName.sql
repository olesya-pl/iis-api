/*
исправляет PropertyName в строка которые имеют неправильные значения
*/

@set wrongValue = '.System.Collections.Generic.Dictionary`2[System.String,System.Object]'
@set criteria = '%.System.Collections.Generic.Dictionary`2[System.String,System.Object]%'

update public."ChangeHistory"
set "PropertyName" = replace ("PropertyName", :wrongValue, '')
where "PropertyName" like :criteria;

