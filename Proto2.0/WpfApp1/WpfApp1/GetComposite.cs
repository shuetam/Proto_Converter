using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class GetComposite
    {
        public static void CompareComposites(Composite comp1, Composite comp2)
        {
            int count = ((comp1.Children.Count < comp2.Children.Count) ? comp1.Children.Count : comp2.Children.Count);

            Composite shorter = ((comp1.Children.Count < comp2.Children.Count) ? comp1 : comp2);

            for (int i = 0; i < count; i++)
            {

                var c1 = (Composite)comp1.Children[i];
                var c2 = (Composite)comp2.Children[i];

                int diffrent = Math.Abs(comp1.Children.Count - comp2.Children.Count);

                if (c1.value != null && c2.value != null)
                {

                    if (c1.value.ToString() != c2.value.ToString())
                    {
                        c1.SubItemBackground = "Red";
                        c2.SubItemBackground = "Red";
                    }
                }


                for (int j = 0; j < diffrent; j++)
                {
                    Composite empty = new Composite("Empty", null);
                    empty.SubItemBackground = "Yellow";
                    shorter.Add(empty);
                }

                c1.parent = comp1;
                c2.parent = comp2;

                CompareComposites(c1, c2);

            }

        }

    }
}
