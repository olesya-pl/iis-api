/*
выводит строки  которых 3 координаты и вторая равно "0"
*/
select "Id", "Value"
from "Attributes"
where "Value" like '{"type":"Point",%'
and json_array_length(("Value"::json) -> 'coordinates') = 3
and (((("Value"::json) -> 'coordinates') ->> 1)::int) = 0;
