using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp1
{
    public abstract class Component

    {
        public string propertyName { get; set; }
        public object value { get; set; }
        public Composite parent { get; set; }
        public string SubItemBackground { get; set; }
      //  Collections.Generic.List
        public Component(string name, object _value)
        {
            this.propertyName = name;
            //if ((_value.GetType().IsValueType || _value.GetType().FullName == "System.String") && _value != null)
            if ( _value != null)
            {

                if ((_value.GetType().IsValueType || _value.GetType().FullName == "System.String") )
                {
                    this.value = _value;
                }
                if(Regex.IsMatch(name, "Collections.Generic.List"))
                {
                    propertyName = _value.GetType().Name;
                }
            }
            else
            {
                this.value = "";
            }
        }
        public abstract void Add(Component c);
    }
}
