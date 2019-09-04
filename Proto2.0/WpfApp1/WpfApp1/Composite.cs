using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Composite : Component

    {
        public ObservableCollection<Component> Children { get; set; }
        public bool expandStatus { get { return _subItemExpanded; } }
        public bool _subItemExpanded;
        public delegate void SubItemExpandedChanged(Composite item);
        public SubItemExpandedChanged ExpandedChanged;


        public bool SubItemExpanded
        {
            get
            {
                return _subItemExpanded;
            }
            set
            {
                if (_subItemExpanded == value)
                    return;
                _subItemExpanded = value;
                ExpandedChanged?.Invoke(this);
            }
        }

        public Composite(string name, object value) : base(name, value)
        {
            this.Children = new ObservableCollection<Component>();
        }

        public override void Add(Component component1)
        {
            Children.Add(component1);
        }

    }
}
