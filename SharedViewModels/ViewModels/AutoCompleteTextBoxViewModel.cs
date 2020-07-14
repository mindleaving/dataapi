using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using AutoCompleteMatchers;
using DataAPI.Client.Repositories;
using SharedViewModels.Objects;

namespace SharedViewModels.ViewModels
{
    public class AutoCompleteTextBoxViewModel<T> : NotifyPropertyChangedBase, IAutoCompleteTextBoxViewModel where T: class 
    {
        private readonly Func<T, string> displayNameFunc;
        private readonly IReadonlyObjectDatabase<T> objectDatabase;
        private readonly IAutoCompleteMatcher<T> autoCompleteMatcher;
        private readonly int maxSuggestions;
        private readonly Func<string, T> objectBuilder;
        private readonly Timer updateSuggestionsTimer;

        public AutoCompleteTextBoxViewModel(
            Func<T, string> displayNameFunc,
            IReadonlyObjectDatabase<T> objectDatabase,
            IAutoCompleteMatcher<T> autoCompleteMatcher,
            int maxSuggestions = 10,
            bool allowNewValue = false,
            Func<string, T> objectBuilder = null)
        {
            if(allowNewValue && objectBuilder == null)
                throw new ArgumentNullException(nameof(objectBuilder), "Object builder must be set, if new values are allowed");

            this.displayNameFunc = displayNameFunc;
            this.objectDatabase = objectDatabase;
            this.autoCompleteMatcher = autoCompleteMatcher;
            this.maxSuggestions = maxSuggestions;
            AcceptsNewValues = allowNewValue;
            this.objectBuilder = objectBuilder;
            updateSuggestionsTimer = new Timer(state => UpdateSuggestions());

            SelectNextCommand = new RelayCommand(SelectNextSuggestion);
            SelectPreviousCommand = new RelayCommand(SelectPreviousSuggestion);
            CloseSuggestionsCommand = new RelayCommand(() => ShowSuggestions = false);

            SetBorderBrush();
        }

        public bool AcceptsNewValues { get; }

        private bool isNewValue = true;
        public bool IsNewValue
        {
            get => isNewValue;
            private set
            {
                isNewValue = value;
                OnPropertyChanged();
            }
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                if(value == searchText)
                    return;
                searchText = value;
                if(selectedExistingDisplayNameObject != null 
                   && searchText != SearchTextFromObject((T)selectedExistingDisplayNameObject.Object))
                {
                    selectedExistingDisplayNameObject = null;
                }
                SetBorderBrush();
                updateSuggestionsTimer.Change(500, Timeout.Infinite);
                OnPropertyChanged();
            }
        }

        private Color borderColor;
        public Color BorderColor
        {
            get => borderColor;
            private set
            {
                borderColor = value;
                OnPropertyChanged();
            }
        }

        private int borderThickness;
        public int BorderThickness
        {
            get => borderThickness;
            private set
            {
                borderThickness = value;
                OnPropertyChanged();
            }
        }

        private List<ObjectWithDisplayName> suggestedObjects;
        public List<ObjectWithDisplayName> SuggestedObjects
        {
            get => suggestedObjects;
            private set
            {
                suggestedObjects = value;
                ShowSuggestions = suggestedObjects.Any();
                OnPropertyChanged();
            }
        }
        private ObjectWithDisplayName selectedExistingDisplayNameObject;
        public ObjectWithDisplayName SelectedDisplayNameObject
        {
            get
            {
                if(selectedExistingDisplayNameObject != null)
                    return selectedExistingDisplayNameObject;
                if (AcceptsNewValues && !string.IsNullOrWhiteSpace(SearchText))
                    return new ObjectWithDisplayName(objectBuilder(SearchText), SearchText);
                return null;
            }
            set
            {
                selectedExistingDisplayNameObject = value;
                if(selectedExistingDisplayNameObject != null)
                {
                    SearchText = selectedExistingDisplayNameObject.DisplayName;
                    ShowSuggestions = keepSuggestionsOpen;
                }
                IsNewValue = selectedExistingDisplayNameObject == null;
                SetBorderBrush();
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedObject));
            }
        }

        private void SetBorderBrush()
        {
            if (AcceptsNewValues)
            {
                BorderColor = new Color("#696969");
                BorderThickness = 1;
            }
            else if (SelectedDisplayNameObject != null)
            {
                BorderColor = new Color("#008000");
                BorderThickness = 1;
            }
            else if (!string.IsNullOrEmpty(SearchText))
            {
                BorderColor = new Color("#FF0000");
                BorderThickness = 2;
            }
            else
            {
                BorderColor = new Color("#696969");
                BorderThickness = 1;
            }
        }

        public T SelectedObject
        {
            get => (T) SelectedDisplayNameObject?.Object;
            set
            {
                SelectedDisplayNameObject = value != null
                    ? new ObjectWithDisplayName(value, SearchTextFromObject(value))
                    : null;
            }
        }

        private string SearchTextFromObject(T obj)
        {
            return displayNameFunc(obj);
        }

        private bool showSuggestions;
        public bool ShowSuggestions
        {
            get => showSuggestions;
            private set
            {
                showSuggestions = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand CloseSuggestionsCommand { get; }

        private bool keepSuggestionsOpen;
        private void SelectPreviousSuggestion()
        {
            if(!SuggestedObjects.Any())
                return;
            if (selectedExistingDisplayNameObject == null)
            {
                SelectedDisplayNameObject = SuggestedObjects.Last();
                ShowSuggestions = true;
                return;
            }
            var indexOfSelected = SuggestedObjects.IndexOf(selectedExistingDisplayNameObject);
            if(indexOfSelected < 0)
                return;
            if(indexOfSelected == 0)
                return;
            keepSuggestionsOpen = true; // Selecting a suggestion should by default close suggestions. Suppress it here.
            SelectedDisplayNameObject = SuggestedObjects[indexOfSelected - 1];
            keepSuggestionsOpen = false; // Restore close of suggestions in any other context
        }

        private void SelectNextSuggestion()
        {
            if(!SuggestedObjects.Any())
                return;
            if (selectedExistingDisplayNameObject == null)
            {
                SelectedDisplayNameObject = SuggestedObjects.First();
                ShowSuggestions = true;
                return;
            }
            var indexOfSelected = SuggestedObjects.IndexOf(selectedExistingDisplayNameObject);
            if(indexOfSelected < 0)
                return;
            if(indexOfSelected == SuggestedObjects.Count-1)
                return;
            keepSuggestionsOpen = true; // Selecting a suggestion should by default close suggestions. Suppress it here.
            SelectedDisplayNameObject = SuggestedObjects[indexOfSelected + 1];
            keepSuggestionsOpen = false; // Restore close of suggestions in any other context
        }


        private bool isUpdating;
        private async void UpdateSuggestions()
        {
            if(isUpdating)
                return;
            isUpdating = true;
            try
            {
                if (selectedExistingDisplayNameObject != null) // Don't update if a batch is selected
                    return;
                if (SearchText.Length < 3)
                {
                    SuggestedObjects = new List<ObjectWithDisplayName>();
                    return;
                }
                var matchingObjects = (await autoCompleteMatcher
                    .FindMatches(objectDatabase, SearchText, maxSuggestions)).OrderByDescending(obj => autoCompleteMatcher.MatchQuality(obj, SearchText));
                SuggestedObjects = matchingObjects
                    .Take(maxSuggestions)
                    .Select(obj => new ObjectWithDisplayName(obj, SearchTextFromObject(obj)))
                    .ToList();
                ShowSuggestions = SuggestedObjects.Any();
            }
            finally
            {
                isUpdating = false;
            }
        }
    }
}