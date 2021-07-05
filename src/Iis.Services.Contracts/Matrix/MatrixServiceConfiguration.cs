using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Services.Contracts.Matrix
{
    public class MatrixServiceConfiguration
    {
        public bool Enabled { get; set; }
        public string Server { get; set; }
        public bool CreateUsers { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
