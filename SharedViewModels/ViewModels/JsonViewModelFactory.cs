using System;
using Newtonsoft.Json.Linq;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;

namespace SharedViewModels.ViewModels
{
    public class JsonViewModelFactory
    {
        private readonly IClipboard clipboard;
        private readonly ICollectionSwitcher collectionSwitcher;

        public JsonViewModelFactory(IClipboard clipboard, ICollectionSwitcher collectionSwitcher)
        {
            this.clipboard = clipboard;
            this.collectionSwitcher = collectionSwitcher;
        }

        public IJsonViewModel Create(JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Object:
                    return new JObjectViewModel((JObject) jToken, this);
                //case JTokenType.Array:
                //    break;
                case JTokenType.Property:
                    return new JPropertyViewModel((JProperty)jToken, this, clipboard, collectionSwitcher);
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    return new JValueViewModel(jToken.ToString());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}