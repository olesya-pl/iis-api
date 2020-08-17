using System;
using System.Collections.Generic;
using Iis.Interfaces.Roles;

namespace Iis.Services.Contracts
{
    public class AccessGranted
    {
        public const string CreateAccessName = "create";
        public const string ReadAccessName = "read";
        public const string UpdateAccessName = "update";
        public const string DeleteAccessName = "delete";

        public Guid Id { get; set; }
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
                if (CreateGranted) list.Add(CreateAccessName);
                if (ReadGranted) list.Add(ReadAccessName);
                if (UpdateGranted) list.Add(UpdateAccessName);
                if (DeleteGranted) list.Add(DeleteAccessName);
                return list;
            }
        }

        public AccessGranted Add(AccessGranted other)
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
