using Commons.Extensions;
using Commons.Physics;

namespace SharedViewModels.Extensions
{
    public static class UnitValueExtensions
    {
        public static string PresentVolume(this UnitValue remainingVolume)
        {
            var targetUnit = Unit.Liter;
            var volumeInLiters = remainingVolume.In(targetUnit);
            var siPrefix = volumeInLiters.SelectSIPrefix();
            volumeInLiters /= siPrefix.GetMultiplier();
            return $"{volumeInLiters:F0}{siPrefix.StringRepresentation()}{targetUnit.StringRepresentation}";
        }
    }
}
