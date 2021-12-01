select q.*
from
(select lh.*, row_number() over(partition by "EntityId" order by "RegisteredAt" desc) as rnum
from "LocationHistory" lh
  inner join "Nodes" n on n."Id" = lh."EntityId"
where not n."IsArchived" and n."NodeTypeId" in ({NodeTypeIds})) q
where q.rnum = 1