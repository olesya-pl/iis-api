﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Iis.OntologyManager.Style
{
    public class OntologyManagerStyle : IOntologyManagerStyle
    {
        public int MarginVer { get; set; }
        public int MarginVerSmall { get; set; }
        public int MarginHor { get; set; }
        public int ControlWidthDefault { get; set; }
        public Color BackgroundColor { get; set; }
        public Color EntityTypeBackColor { get; set; }
        public Color AttributeTypeBackColor { get; set; }
        public Color RelationTypeBackColor { get; set; }
        public Font SelectedFont { get; set; }
        public static IOntologyManagerStyle GetDefaultStyle()
        {
            return new OntologyManagerStyle
            {
                MarginVer = 4,
                MarginVerSmall = 2,
                MarginHor = 10,
                ControlWidthDefault = 200,
                BackgroundColor = Color.LightYellow,
                EntityTypeBackColor = Color.Khaki,
                AttributeTypeBackColor = Color.PaleGreen,
                RelationTypeBackColor = Color.Lavender,
            };
        }
    }
}
