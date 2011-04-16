## Dependency Property Weaver

This project grew out of the need to easily work with creating properties for use with WPF.  There are 2 ways to implement change notification: INotifyPropertyChanged and Dependency Properties.

I chose the route of generating Dependency Properties with post build IL weaving as DPs are much more performant with WPF.

The code is work in progress but at this point it is capable of converting any POCO object into a DependencyObject, and converting auto properties into dependency properties.

As a basic example, the following code:

public class Student {
  public int Age { get; set; }
}

Will get weaved into this:

public class Student : DependencyObject {
  public static readonly DependencyProperty AgeProperty = DependencyProperty.Register("Age", typeof(int), typeof(Student));
  public int Age {
    get { return (int)GetValue(AgeProperty); }
    set { SetValue(AgeProperty, value); }
  }
}



All source code is released under the [Ms-PL](http://www.opensource.org/licenses/ms-pl) license.
