using System;
using System.Linq;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public class FormatAttributeService : IFormatAttributeService
    {
        private readonly FileUrlGetter _fileUrlGetter;

        public FormatAttributeService(FileUrlGetter fileUrlGetter)
        {
            _fileUrlGetter = fileUrlGetter;
        }

        public IFormatProvider Iso8601DateFormat { get; private set; }

        public object FormatValue(ScalarType scalarType, string value)
        {
            switch (scalarType)
            {
                case ScalarType.Int:
                    return Convert.ToInt32(value);
                case ScalarType.Date:
                    {
                        if (DateTime.TryParse(value, out DateTime dateTimeValue))
                        {
                            return dateTimeValue.ToString(Iso8601DateFormat);
                        }
                        else
                        {
                            return value;
                        }
                    }
                case ScalarType.File:
                    return new
                    {
                        fileId = value,
                        url = _fileUrlGetter.GetFileUrl(new Guid(value))
                    };
                case ScalarType.IntegerRange:
                case ScalarType.FloatRange:
                    throw new NotSupportedException("To format range types use FormatRange");
                default:
                    return value;
            }
        }

        public object FormatRange(string value)
        {
            var splitted = value.Split('-', ' ', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Count() == 1 || splitted.Count() == 2)
            {
                var firstString = splitted.First();
                var lastString = splitted.Last();

                if (decimal.TryParse(firstString, out var first) && decimal.TryParse(lastString, out var last))
                {
                    return new
                    {
                        gte = first,
                        lte = last
                    };
                }
            }
            return null;
        }
    }
}