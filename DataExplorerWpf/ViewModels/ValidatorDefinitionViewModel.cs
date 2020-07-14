using System.Collections.Generic;
using System.Linq;
using DataAPI.DataStructures.Validation;
using SharedViewModels.Extensions;
using SharedViewModels.ViewModels;

namespace DataExplorerWpf.ViewModels
{
    public class ValidatorDefinitionViewModel : NotifyPropertyChangedBase
    {
        public ValidatorDefinitionViewModel(ValidatorDefinition model)
        {
            Model = model;
            IsApproved = model.IsApproved;
            RulesSingleLineTruncated = model.Ruleset.Truncate(50);
            Rules = model.Ruleset?.Split(';').Select(x => x.Trim()).ToList() ?? new List<string>();
        }

        public ValidatorDefinition Model { get; }

        private bool isApproved;
        public bool IsApproved
        {
            get => isApproved;
            set
            {
                isApproved = value;
                OnPropertyChanged();
            }
        }
        public string Submitter => Model.Submitter;
        public IList<string> Rules { get; }
        public string RulesSingleLineTruncated { get; }
    }
}
