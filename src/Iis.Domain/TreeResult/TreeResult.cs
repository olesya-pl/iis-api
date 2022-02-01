using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Domain.TreeResult
{
    public class TreeResult
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public List<TreeResult> Options { get; set; } = new List<TreeResult>();

        public override string ToString() => Label;

        public JObject GetJsonObject(string labelName, string valueName, string optionsName)
        {
            var result = new JObject();
            result[labelName] = Label;

            if (!string.IsNullOrEmpty(Value))
                result[valueName] = Value;

            if (Options.Count > 0)
            {
                var jOptions = new JArray();
                foreach (var option in Options)
                {
                    jOptions.Add(option.GetJsonObject(labelName, valueName, optionsName));
                }
                result[optionsName] = jOptions;
            }

            return result;
        }

        public TreeResult Init<T>(
            T item,
            Func<T, string> labelFunc,
            Func<T, string> valueFunc,
            Func<T, IReadOnlyList<T>> optionsFunc)
        {
            Label = labelFunc(item);
            Value = valueFunc(item);
            Options = optionsFunc(item)
                .Select(_ => new TreeResult().Init(_, labelFunc, valueFunc, optionsFunc))
                .ToList();
            return this;
        }
    }
}
