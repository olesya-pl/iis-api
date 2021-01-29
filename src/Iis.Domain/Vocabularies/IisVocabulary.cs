using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Vocabularies
{
    public class IisVocabulary
    {
        Dictionary<string, string> _dict = new Dictionary<string, string>
        {
            { "Importance", "Важливість" },
            { "Reliability", "Достовірність" },
            { "Relevance", "Актуальність" },
            { "Completeness", "Повнота" },
            { "SourceReliability", "Надійність джерела" },
            { "ProcessedStatus", "Статус обробки" },
            { "SessionPriority", "Прiоритет сеансу" },
            { "Assignee", "Виконавець" },
            { "Content", "Зміст" },
            { "MaterialFeature.NodeId", "Зв'язок"}
        };

        public string Translate(string key)
        {
            return _dict.ContainsKey(key) ? _dict[key] : key;
        }
    }
}
