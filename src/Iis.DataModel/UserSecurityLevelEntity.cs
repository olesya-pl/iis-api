using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel
{
    public class UserSecurityLevelEntity : BaseEntity
    {
        public Guid UserId { get; set; }
        public UserEntity User { get; set; }
        public int SecurityLevelIndex { get; set; }
    }
}
