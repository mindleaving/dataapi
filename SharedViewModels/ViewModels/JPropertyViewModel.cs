using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using SharedViewModels.Helpers;
using SharedViewModels.Objects;

namespace SharedViewModels.ViewModels
{
    public class JPropertyViewModel : IJsonViewModel
    {
        private readonly IClipboard clipboard;
        private readonly ICollectionSwitcher collectionSwitcher;

        public JPropertyViewModel(
            JProperty jProperty, 
            JsonViewModelFactory jsonViewModelFactory, 
            IClipboard clipboard,
            ICollectionSwitcher collectionSwitcher)
        {
            JToken = jProperty;
            this.clipboard = clipboard;
            this.collectionSwitcher = collectionSwitcher;
            Name = jProperty.Name;
            var referenceMatch = Regex.Match(Name, "^(?<ReferenceCollection>.+)(Id|ID)$");
            IsReference = referenceMatch.Success;
            ReferencedCollection = DetermineReferencedCollection(referenceMatch);
            if (jProperty.Value.HasValues)
            {
                Value = string.Empty;
                if (jProperty.Value.Type == JTokenType.Array)
                {
                    Children = jProperty.Value.Children()
                        .Select(jsonViewModelFactory.Create)
                        .Where(vm => vm != null)
                        .ToList();
                }
                else if(jProperty.Value.Type == JTokenType.Object)
                {
                    Children = ((JObject) jProperty.Value)
                        .Children()
                        .Select(jsonViewModelFactory.Create)
                        .ToList();
                }
            }
            else
            {
                Value = jProperty.Value.ToString();
                Children = new List<IJsonViewModel>();
            }
            CopyNameCommand = new RelayCommand(CopyNameToClipboard);
            CopyValueCommand = new RelayCommand(CopyValueToClipboard);
            OpenReferenceCommand = new RelayCommand(OpenReference, () => IsReference);
        }

        private static string DetermineReferencedCollection(Match referenceMatch)
        {
            if (!referenceMatch.Success)
                return null;
            var collectionName = referenceMatch.Groups["ReferenceCollection"].Value;
            if (collectionName == "Plate")
                return "Plate";
            return collectionName;
        }

        public bool IsReference { get; }
        public string ReferencedCollection { get; }

        public JToken JToken { get; }
        public string Name { get; }
        public string Value { get; }
        public List<IJsonViewModel> Children { get; }

        public ICommand CopyNameCommand { get; }
        public ICommand CopyValueCommand { get; }
        public ICommand OpenReferenceCommand { get; }

        private void CopyNameToClipboard()
        {
            clipboard.SetText(Name);
        }

        private void CopyValueToClipboard()
        {
            clipboard.SetText(Value);
        }

        private void OpenReference()
        {
            collectionSwitcher.SwitchTo(ReferencedCollection, $"SELECT *\r\nWHERE _id = '{Value}'");
        }
    }
}
