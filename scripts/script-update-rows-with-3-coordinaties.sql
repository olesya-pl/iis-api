/*
обновляет строки  которых 3 координаты и вторая равно "0" - удаляя координату со значением "0"
*/
update "Attributes"
set "Value" = replace ("Value" , ',0,', ',')
where "Value" like '{"type":"Point",%'
and json_array_length(("Value"::json) -> 'coordinates') = 3
and (((("Value"::json) -> 'coordinates') ->> 1)::int) = 0;
