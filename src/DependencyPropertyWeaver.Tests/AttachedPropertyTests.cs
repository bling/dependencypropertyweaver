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
        public void Static_DependencyProperties_Generate_Getter_And_Setter()
        {
            Attached.SetHeight(base.Window, 100);
            Assert.AreEqual(100, Attached.GetHeight(base.Window));

            Attached.SetWidth(base.Window, 100);
            Assert.AreEqual(100, Attached.GetWidth(base.Window));

            Attached.SetText(base.Window, "test");
            Assert.AreEqual("test", Attached.GetText(base.Window));
        }
    }
}