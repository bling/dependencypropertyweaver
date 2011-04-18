using DependencyPropertyWeaver.Tests.Models;
using DependencyPropertyWeaver.Tests.Wpf;
using NUnit.Framework;

namespace DependencyPropertyWeaver.Tests
{
    [TestFixture]
    [RequiresSTA]
    internal class AttachedPropertyTests : WpfTestBase<AttachedWindow>
    {
        [Test]
        public void Window_Can_Be_Shown()
        {
            // implies loading of the XAML was possible
        }

        [Test]
        public void Static_auto_props_are_weaved_into_methods()
        {
            Attached.SetHeight(base.Window, 100);
            Assert.AreEqual(100, Attached.GetHeight(base.Window));
        }
    }
}