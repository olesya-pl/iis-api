using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Roles
{
    public class AccessGranted : IAccessGranted
    {
        public AccessKind Kind { get; set; }
        public AccessCategory Category { get; set; }
        public string Title { get; set; }
        public bool CreateGranted { get; set; }
        public bool ReadGranted { get; set; }
        public bool UpdateGranted { get; set; }
        public bool DeleteGranted { get; set; }
        public List<string> AllowedOperations
        {
            get
            {
                var list = new List<string>();
                if (CreateGranted) list.Add("create");
                if (ReadGranted) list.Add("read");
                if (UpdateGranted) list.Add("update");
                if (DeleteGranted) list.Add("delete");
                return list;
            }
        }

        public AccessGranted Add(IAccessGranted other)
        {
            CreateGranted |= other.CreateGranted;
            ReadGranted |= other.ReadGranted;
            UpdateGranted |= other.UpdateGranted;
            DeleteGranted |= other.DeleteGranted;
            return this;
        }

        public bool IsGranted(AccessOperation operation)
        {
            switch (operation)
            {
                case AccessOperation.Create:
                    return CreateGranted;
                case AccessOperation.Read:
                    return ReadGranted;
                case AccessOperation.Update:
                    return UpdateGranted;
                case AccessOperation.Delete:
                    return DeleteGranted;
            }
            return false;
        }
    }
}
