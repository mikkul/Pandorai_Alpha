using Newtonsoft.Json;
using Pandorai.Persistency.Converters;

namespace Pandorai.Conditions
{
    [JsonConverter(typeof(ConditionConverter))]
    public abstract class Condition
    {
        public string TypeName => GetType().Name;

        public abstract bool Check();
        public abstract void SetAttribute(string name, string value);
    }
}