using System;
using Commons.Physics;

namespace SharedViewModels.Helpers
{
    public static class UnitValueHelpers
    {
        public static UnitValue DefaultDensity { get; } = new UnitValue(CompoundUnits.Kilogram / CompoundUnits.CubicMeters, 1000);

        public static bool IsValidUnit(string batchUnit)
        {
            if (batchUnit == null)
                return false;
            if (string.IsNullOrWhiteSpace(batchUnit))
                return false;
            try
            {
                UnitValue.Parse($"0 {batchUnit}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts mass and volume unit values to mass. Incompatible units will throw exception.
        /// </summary>
        /// <param name="unitValue">Unit value in unknown units</param>
        /// <param name="density">Density used if volumes need to be converted</param>
        /// <returns>Unit value in mass units</returns>
        public static UnitValue ConvertToMass(UnitValue unitValue, UnitValue density)
        {
            if (unitValue.CanConvertTo(Unit.Kilogram))
                return unitValue;
            if (unitValue.CanConvertTo(Unit.CubicMeters))
                return unitValue * density;
            throw new NotSupportedException($"Conversion of '{unitValue}' to mass is not supported");
        }
    }
}
