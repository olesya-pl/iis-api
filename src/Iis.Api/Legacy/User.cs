﻿using System;
using System.Collections.Generic;

namespace IIS.Legacy.EntityFramework
{
    public partial class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}