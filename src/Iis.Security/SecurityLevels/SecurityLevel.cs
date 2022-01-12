using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Iis.Interfaces.SecurityLevels;

[assembly: InternalsVisibleTo("Iis.UnitTests")]

namespace Iis.Security.SecurityLevels
{
    internal class SecurityLevel: ISecurityLevel
    {
        public Guid Id { get;  set; }
        public string Name { get; set; }
        public int UniqueIndex { get; set; }
        internal SecurityLevel _parent;
        public ISecurityLevel Parent => _parent;
        internal List<SecurityLevel> _children = new List<SecurityLevel>();
        public IReadOnlyList<ISecurityLevel> Children => _children;
        public SecurityLevel() { }
        public override string ToString() => $"{UniqueIndex} : {Name}";
        internal SecurityLevel(string name, int uniqueIndex, IReadOnlyList<SecurityLevel> children = null)
        {
            Id = new Guid();
            Name = name;
            UniqueIndex = uniqueIndex;
            if (children != null)
            {
                _children = new List<SecurityLevel>(children);
                foreach (var child in _children)
                {
                    child._parent = this;
                }
            }
        }

        internal bool IsRoot => _parent == null;
        internal SecurityLevel Root => IsRoot ? this : _parent.Root;
        internal IReadOnlyList<SecurityLevel> GetItems(IReadOnlyList<int> indexes = null)
        {
            var result = new List<SecurityLevel>();
            if (indexes == null || indexes.Contains(UniqueIndex))
            {
                result.Add(this);
            }

            _children.ForEach(_ => result.AddRange(_.GetItems(indexes)));
            return result;
        }
        internal IReadOnlyList<SecurityLevel> GetAllItems(IReadOnlyList<int> indexes = null) => 
            IsRoot ? GetItems(indexes) : Root.GetItems(indexes);
        internal SecurityLevel GetItem(int uniqueIndex) => GetAllItems().SingleOrDefault(_ => _.UniqueIndex == uniqueIndex);
        internal SecurityLevel GetItem(Guid id) => GetAllItems().SingleOrDefault(_ => _.Id == id);
        internal bool IsParentOf(SecurityLevel level) => 
            level._parent != null && (level._parent == this || IsParentOf(level._parent));
        internal bool IsChildOf(SecurityLevel level) => level.IsParentOf(this);
        internal bool IsBrotherOf(SecurityLevel level) => level != this && _parent == level._parent;
    }
}
