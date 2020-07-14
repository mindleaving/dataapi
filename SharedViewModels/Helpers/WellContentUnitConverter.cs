using System;
using Commons.Extensions;
using Commons.Physics;

namespace SharedViewModels.Helpers
{
    /// <summary>
    /// This calculates volumes from units which aren't
    /// necessarily compatible and not enough information
    /// is known to make a scientifically correct volume calculation
    /// </summary>
    public static class WellContentUnitConverter
    {
        public static double ExtractLiterEquivalentFromAmount(UnitValue amount)
        {
            if (amount.CanConvertTo(Unit.Liter))
                return amount.In(Unit.Liter);
            if (amount.CanConvertTo(Unit.Kilogram))
                return amount.In(Unit.Kilogram);
            throw new NotSupportedException();
        }
    }
}
