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
        internal SecurityLevel _parent;
        internal List<SecurityLevel> _children = new List<SecurityLevel>();

        public SecurityLevel() { }
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

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int UniqueIndex { get; set; }
        public int? ParentUniqueIndex => _parent?.UniqueIndex;
        public ISecurityLevel Parent => _parent;
        public IReadOnlyList<ISecurityLevel> Children => _children;
        public bool IsNew => UniqueIndex == -1;

        public override string ToString() => $"{UniqueIndex} : {Name}";
        public bool IsGroup => _parent?.IsRoot == true;

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
        internal int GetDepth()
        {
            int depth = 1;
            var level = _parent;
            while (level != null)
            {
                depth++;
                level = level._parent;
            }
            return depth;
        }
    }
}
