using Iis.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DbLayer.Common
{
    public class EqualityByIdComparer: IEqualityComparer<BaseEntity>
    {
        public bool Equals(BaseEntity entity1, BaseEntity entity2) =>
            entity1?.Id == entity2?.Id;

        public int GetHashCode(BaseEntity entity) => entity.Id.GetHashCode();
    }
}
