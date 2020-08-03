using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using SharedViewModels.Helpers;

namespace SharedViewModels.ViewModels
{
    public class UnitValueEditViewModel : NotifyPropertyChangedBase
    {
        private bool suppressAmountPropertyChangedEvent;

        public UnitValueEditViewModel(UnitValue amount = null, IList<IUnitDefinition> supportedUnits = null)
        {
            if(amount != null)
                AmountText = amount.ToString();
            if (supportedUnits != null)
            {
                Units = supportedUnits;
                RestrictToSupportedUnits = true;
            }
        }

        public UnitValue Amount
        {
            get
            {
                var multiplier = SelectedMultiplier.GetMultiplier();
                var value = multiplier * AmountValue;
                return SelectedUnit != null
                    ? value.To(SelectedUnit)
                    : new UnitValue(new CompoundUnit(), value);
            }
            set
            {
                if(value != null)
                    AmountText = value.ToString();
                else
                {
                    SelectedUnit = null;
                    OnPropertyChanged(nameof(AmountText));
                }
                OnPropertyChanged(nameof(Amount));
            }
        }

        private bool RestrictToSupportedUnits { get; }

        private double amountValue;
        public double AmountValue => amountValue;

        public string AmountText
        {
            get => amountValue.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out amountValue))
                {
                    suppressAmountPropertyChangedEvent = true;
                    try
                    {
                        var unitValue = UnitValue.Parse(value);
                        if(unitValue == null)
                            throw new FormatException();
                        if (RestrictToSupportedUnits)
                        {
                            var matchingUnit = Units.Where(unit => unitValue.CanConvertTo(unit)).ToList();
                            if (!matchingUnit.Any())
                            {
                                StaticMessageBoxSpawner.Show($"The entered value '{value}' is valid, but doesn't match any of the supported units");
                                throw new NotSupportedException();
                            }

                            SelectedUnit = matchingUnit.First();
                            var valueInMatchingUnit = unitValue.In(SelectedUnit);
                            SelectedMultiplier = valueInMatchingUnit.SelectSIPrefix();
                            amountValue = valueInMatchingUnit / SelectedMultiplier.GetMultiplier();
                        }
                        else
                        {
                            SelectedUnit = Unit.Effective.AllUnits.FirstOrDefault(
                                x => x.IsSIUnit && x.CorrespondingCompoundUnit == unitValue.Unit);
                            SelectedMultiplier = unitValue.Value.SelectSIPrefix();
                            amountValue = unitValue.Value / SelectedMultiplier.GetMultiplier();
                        }
                    }
                    catch (FormatException)
                    {
                        var lowerCaseValue = value.Trim().ToLowerInvariant();
                        if (lowerCaseValue.All(c => !char.IsWhiteSpace(c)))
                        {
                            if (lowerCaseValue.StartsWith("inf") || lowerCaseValue.StartsWith("+inf"))
                                amountValue = double.PositiveInfinity;
                            else if (lowerCaseValue.StartsWith("-inf"))
                                amountValue = double.NegativeInfinity;
                            else
                                throw new FormatException();
                        }
                        else
                            throw new FormatException();
                    }
                    finally
                    {
                        suppressAmountPropertyChangedEvent = false;
                    }
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(AmountValue));
                OnPropertyChanged(nameof(Amount));
            }
        }

        public IList<IUnitDefinition> Units { get; } = Unit.Effective.AllUnits.ToList();
        private IUnitDefinition selectedUnit;
        public IUnitDefinition SelectedUnit
        {
            get => selectedUnit;
            set
            {
                selectedUnit = value;
                OnPropertyChanged();
                if(!suppressAmountPropertyChangedEvent)
                    OnPropertyChanged(nameof(Amount));
            }
        }

        public IList<SIPrefix> Multipliers { get; } = (SIPrefix[]) Enum.GetValues(typeof(SIPrefix));
        private SIPrefix selectedMultiplier = SIPrefix.None;
        public SIPrefix SelectedMultiplier
        {
            get => selectedMultiplier;
            set
            {
                selectedMultiplier = value;
                OnPropertyChanged();
                if(!suppressAmountPropertyChangedEvent)
                    OnPropertyChanged(nameof(Amount));
            }
        }
    }
}
