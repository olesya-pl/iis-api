﻿using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Roles
{
    public class AccessGrantedList: List<AccessGranted>
    {
        public AccessGrantedList Merge(IEnumerable<AccessGranted> otherList)
        {
            foreach (var otherItem in otherList)
            {
                var localItem = this.SingleOrDefault(ag => ag.Kind == otherItem.Kind && ag.Category == otherItem.Category);
                if (localItem == null)
                {
                    this.Add(otherItem);
                }
                else
                {
                    localItem.Add(otherItem);
                }
            }
            return this;
        }

        public bool IsGranted(AccessKind kind, AccessOperation operation)
        {
            var item = this.SingleOrDefault(ag => ag.Kind == kind);
            return item == null ? false : item.IsGranted(operation);
        }
    }
}