using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Pandorai.Effects;
using Pandorai.Items;

namespace Pandorai.Persistency.Converters
{
    public class EffectSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Effect).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class EffectConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new EffectSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Effect));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string effectId = jo["Id"].Value<string>();
            return JsonConvert.DeserializeObject(jo.ToString(), TypeLegends.Effects[effectId], SpecifiedSubclassConversion);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }
}