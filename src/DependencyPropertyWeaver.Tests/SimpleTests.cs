using System;
using DependencyPropertyWeaver.Tests.Models;
using DependencyPropertyWeaver.Tests.Wpf;
using NUnit.Framework;

namespace DependencyPropertyWeaver.Tests
{
    public class SimpleTests
    {
        [RequiresSTA]
        public class When_Getter_Setter_Auto_Property : WpfTestBase<SimpleWindow>
        {
            [Test]
            public void Static_dependency_property_fields_exist()
            {
                Assert.IsNotNull(Simple.PrimitiveDependencyProperty);
                Assert.IsNotNull(Simple.ReferenceDependencyProperty);
                Assert.IsNotNull(Simple.ValueTypeDependencyProperty);
            }

            [Test]
            public void Primitive_types_are_weaved()
            {
                Assert.AreEqual(Window.ViewModel.Primitive, 0);

                Window.PrimitiveTextBox.Text = "1";
                Assert.AreEqual(Window.ViewModel.Primitive, 1);

                Window.ViewModel.Primitive = 2;
                Assert.AreEqual(Window.PrimitiveTextBox.Text, "2");
            }

            [Test]
            public void Value_types_are_weaved()
            {
                Assert.AreEqual(Window.ViewModel.ValueType, new TimeSpan());

                Window.ValueTypeTextBox.Text = TimeSpan.FromHours(1).ToString();
                Assert.AreEqual(Window.ViewModel.ValueType, TimeSpan.FromHours(1));

                Window.ViewModel.ValueType = TimeSpan.FromHours(2);
                Assert.AreEqual(Window.ValueTypeTextBox.Text, TimeSpan.FromHours(2).ToString());
            }

            [Test]
            public void Reference_types_are_weaved()
            {
                Assert.IsNull(Window.ViewModel.Reference);

                Window.ReferenceTextBox.Text = "123";
                Assert.AreEqual(Window.ViewModel.Reference, "123");

                Window.ViewModel.Reference = "321";
                Assert.AreEqual("321", Window.ReferenceTextBox.Text);
            }
        }

        public class When_Public_Getter_Protected_Setter
        {
            public class Tester : PublicGetterProtectedSetter
            {
                public Tester(int value)
                {
                    X = value;
                }
            }

            [Test]
            public void Protected_setters_are_accessible_to_subclasses()
            {
                var test = new Tester(123);
                Assert.AreEqual(test.X, 123);
            }
        }
    }
}