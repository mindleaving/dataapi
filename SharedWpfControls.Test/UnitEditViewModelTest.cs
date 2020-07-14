using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using NUnit.Framework;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.Test
{
    [TestFixture]
    public class UnitEditViewModelTest
    {
        [Test]
        public void PropertyChangedEventFiredOnce()
        {
            var sut = new UnitValueEditViewModel();
            var eventList = new List<string>();
            sut.PropertyChanged += (sender, args) => eventList.Add(args.PropertyName);

            sut.Amount = 42.To(Unit.Meter);
            Assert.That(eventList.GroupBy(x => x).Max(x => x.Count()), Is.LessThan(2), 
                GenerateErrorText(nameof(UnitValueEditViewModel.Amount), eventList));
            eventList.Clear();

            sut.AmountText = "3 m";
            Assert.That(eventList.GroupBy(x => x).Max(x => x.Count()), Is.LessThan(2),
                GenerateErrorText(nameof(UnitValueEditViewModel.AmountText), eventList));
        }

        private string GenerateErrorText(string propertyName, List<string> eventList)
        {
            var multipleEventProperties = eventList.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key);
            return $"{propertyName}: Events fired more than once for {string.Join(", ", multipleEventProperties)}";
        }
    }
}
