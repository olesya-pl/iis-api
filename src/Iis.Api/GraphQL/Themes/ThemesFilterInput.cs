using System.Collections.Generic;

namespace IIS.Core.GraphQL.Themes
{
    public class ThemesFilterInput
    {
        public List<string> EntityTypeNames { get; set; } = new List<string>();
    }
}
