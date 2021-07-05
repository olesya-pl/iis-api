using System;
using System.Collections.Generic;
using Iis.Interfaces.Roles;

namespace Iis.Domain.Users
{
    public class AccessGranted
    {
        public const string CreateAccessName = "create";
        public const string ReadAccessName = "read";
        public const string UpdateAccessName = "update";
        public const string SearchAccessName = "search";
        public const string CommentingAccessName = "commenting";
        public const string AccessLevelUpdateAccessName = "accessLevelUpdate";
        
        public Guid Id { get; set; }
        public AccessKind Kind { get; set; }
        public AccessCategory Category { get; set; }        

        public string Title { get; set; }
        public bool CreateAllowed { get; set; }
        public bool ReadAllowed { get; set; }
        public bool UpdateAllowed { get; set; }
        public bool SearchAllowed { get; set; }
        public bool CommentingAllowed { get; set; }
        public bool AccessLevelUpdateAllowed { get; set; }

        public bool CreateGranted { get; set; }
        public bool ReadGranted { get; set; }
        public bool UpdateGranted { get; set; }
        public bool SearchGranted { get; set; }
        public bool CommentingGranted { get; set; }
        public bool AccessLevelUpdateGranted { get; set; }

        public List<string> AllowedOperations
        {
            get
            {
                var list = new List<string>();
                if (CreateAllowed) list.Add(CreateAccessName);
                if (ReadAllowed) list.Add(ReadAccessName);
                if (UpdateAllowed) list.Add(UpdateAccessName);
                if (SearchAllowed) list.Add(SearchAccessName);
                if (CommentingAllowed) list.Add(CommentingAccessName);
                if (AccessLevelUpdateAllowed) list.Add(AccessLevelUpdateAccessName);
                return list;
            }
        }

        public List<string> GrantedOperations
        {
            get
            {
                var list = new List<string>();
                if (CreateGranted) list.Add(CreateAccessName);
                if (ReadGranted) list.Add(ReadAccessName);
                if (UpdateGranted) list.Add(UpdateAccessName);
                if (SearchGranted) list.Add(SearchAccessName);
                if (CommentingGranted) list.Add(CommentingAccessName);
                if (AccessLevelUpdateGranted) list.Add(AccessLevelUpdateAccessName);
                return list;
            }
        }

        public AccessGranted AddGranted(AccessGranted other)
        {
            if (other == null)
            {
                return this;
            }               
            
            CreateGranted = other.CreateGranted;
            ReadGranted = other.ReadGranted;
            UpdateGranted = other.UpdateGranted;
            SearchGranted = other.SearchGranted;
            CommentingGranted = other.CommentingGranted;
            AccessLevelUpdateGranted = other.AccessLevelUpdateGranted;
            return this;
        }

        public AccessGranted MergeGranted(AccessGranted other)
        {
            if (other == null)
            {
                return this;
            }

            CreateGranted |= other.CreateGranted;
            ReadGranted |= other.ReadGranted;
            UpdateGranted |= other.UpdateGranted;
            SearchGranted |= other.SearchGranted;
            CommentingGranted |= other.CommentingGranted;
            AccessLevelUpdateGranted |= other.AccessLevelUpdateGranted;
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
                case AccessOperation.Search:
                    return SearchGranted;
                case AccessOperation.Commenting:
                    return CommentingGranted;
                case AccessOperation.AccessLevelUpdate:
                    return AccessLevelUpdateGranted;

            }
            return false;
        }
    }
}
