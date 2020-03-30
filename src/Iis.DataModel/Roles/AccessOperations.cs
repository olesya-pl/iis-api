using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel.Roles
{
    public class AccessOperations : IAccessOperations
    {
        public bool Create { get; private set; }
        public bool Read { get; private set; }
        public bool Update { get; private set; }
        public bool Delete { get; private set; }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="code">CU - create and update, R - read only, RD - read and delete</param>
        public AccessOperations(string code)
        {
            Create = code.Contains('C');
            Read = code.Contains('R');
            Update = code.Contains('U');
            Delete = code.Contains('D');
        }
    }
}
