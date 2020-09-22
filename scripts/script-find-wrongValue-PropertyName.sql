/*
выводит строки которые имеют неправильные значения PropertyName
*/
@set wrongValue = '.System.Collections.Generic.Dictionary`2[System.String,System.Object]'
@set criteria = '%.System.Collections.Generic.Dictionary`2[System.String,System.Object]%'

SELECT "Id", "PropertyName", replace ("PropertyName", :wrongValue, '') as "Value" 
FROM public."ChangeHistory"
where "PropertyName" like :criteria;
