using System.Collections.Generic;

namespace Iis.Domain.Vocabularies
{
    public class PropertyTranslator
    {
        Dictionary<string, string> _propertyMapping = new Dictionary<string, string>
        {
            { "Importance", "Важливість" },
            { "Reliability", "Достовірність" },
            { "Relevance", "Актуальність" },
            { "Completeness", "Повнота" },
            { "SourceReliability", "Надійність джерела" },
            { "ProcessedStatus", "Статус обробки" },
            { "SessionPriority", "Прiоритет сеансу" },
            { "Assignee", "Виконавець" },
            { "Editor", "Редактор" },
            { "Content", "Зміст" },
            { "MaterialFeature.NodeId", "Зв'язок"}
        };

        public string GetTranslation(string key)
        {
            return _propertyMapping.TryGetValue(key, out string value) ? value : key;
        }
    }
}
