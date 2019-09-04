using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Protobuf;
using Google.ProtocolBuffers;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using Google.Protobuf.Reflection;
using System.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public string[] classContent;
        public string[] dataContent;
        public Dictionary<string, string> protoPaths;
        public List<Type> listOfProtoClasses;
        object proto1;
        object proto2;

        string pathToProtos1 = "";
        string pathToProtos2 = "";

        Composite root1;
        Composite root2;

        Composite selected1;
        Composite selected2;

        public List<object> protoInstances1;
        public List<object> protoInstances2;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void scroll1(object sender, ScrollChangedEventArgs e)
        {
            _viewer2.ScrollToHorizontalOffset(e.HorizontalOffset);
            _viewer2.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void scroll2(object sender, ScrollChangedEventArgs e)
        {
            _viewer1.ScrollToHorizontalOffset(e.HorizontalOffset);
            _viewer1.ScrollToVerticalOffset(e.VerticalOffset);
        }


        private void PreviewMouseWheel1(object sender, MouseWheelEventArgs e)
        {
            _viewer1.ScrollToVerticalOffset(_viewer1.VerticalOffset - e.Delta / 3);
        }

        private void PreviewMouseWheel2(object sender, MouseWheelEventArgs e)
        {
            _viewer2.ScrollToVerticalOffset(_viewer1.VerticalOffset - e.Delta / 3);
        }


        ///////////////////////////// EXPAND EVENT ///////////////////////////////////
        public void expand_Event(Composite item)
        {

            TreeViewL.DataContext = null;
            TreeViewL.DataContext = this.root1;

            TreeViewR.DataContext = null;
            TreeViewR.DataContext = this.root2;

            Composite selectedItem = item as Composite;
            Composite parent = selectedItem.parent as Composite;

            string indexes = "";
            getindexes(selectedItem, ref indexes);

            expandRight(indexes, selectedItem);
            expandLeft(indexes, selectedItem);

        }


        ///////////////////////////// EXPAND EVENT ///////////////////////////////////
        private void getindexes(Composite selectedItem, ref string indexes)
        {
            if (selectedItem != null && selectedItem.parent as Composite != null)
            {
                Composite se = selectedItem.parent as Composite;
                indexes += " " + se.Children.IndexOf(selectedItem);
                getindexes(selectedItem.parent as Composite, ref indexes);
            }
        }


        private void expandLeft(string indexesToExpand, Composite selectedItem)
        {
            if (this.selected1 != null)
                this.selected1.SubItemBackground = "White";

            Composite currentTree = root1;
            if (indexesToExpand != "")
            {
                root2.SubItemExpanded = true;
                string[] indexes = indexesToExpand.Trim().Split();

                int count = indexes.Length;
                for (int i = count - 1; i >= 0; i--)
                {

                    int j = Int32.Parse(indexes[i].ToString());
                    (currentTree.Children[j] as Composite).SubItemExpanded = i == 0 ? selectedItem.expandStatus : true;
                    currentTree = (currentTree.Children[j] as Composite);
                }

            }
            this.selected1 = currentTree;
            this.selected1.SubItemBackground = "Silver";
        }

        private void expandRight(string indexesToExpand, Composite selectedItem)
        {

            if (this.selected2 != null)
                this.selected2.SubItemBackground = "White";

            Composite currentTree = root2;
            if (indexesToExpand != "")
            {
                root2.SubItemExpanded = true;
                string[] indexes = indexesToExpand.Trim().Split();

                int count = indexes.Length;
                for (int i = count - 1; i >= 0; i--)
                {
                    int j = Int32.Parse(indexes[i].ToString());
                    (currentTree.Children[j] as Composite).SubItemExpanded = i == 0 ? selectedItem.expandStatus : true;
                    currentTree = (currentTree.Children[j] as Composite);
                }
            }

            this.selected2 = currentTree;
            this.selected2.SubItemBackground = "Silver";
        }


        public void FillTreesWithObjects(Object obj1, Object obj2)
        {
            Composite root1Proto = new Composite("CLASS1", obj1); /// get roots for objects - first parametr is a main tree header 
            Composite root2proto = new Composite("CLASS2", obj2);
            GetCompositeObject(root1Proto, obj1, obj1); // change root to composites with data from second parametr
            GetCompositeObject(root2proto, obj2, obj2);

            GetComposite.CompareComposites(root1Proto, root2proto);
            this.root1 = root1Proto;
            this.root2 = root2proto;
            TreeViewL.DataContext = this.root1;
            TreeViewR.DataContext = this.root2;
        }

        static string GetObjectName<T>(Expression<Func<T>> expr)
        {
            var body = (MemberExpression)expr.Body;
            return body.Member.Name;
        }

        private TreeViewItem CreateTreeViewItem(object o, int i)
        {
            i = i + 1;
            TreeViewItem item = new TreeViewItem();
            item.Header = o.GetType() + "---" + i;
            if (o is IEnumerable<Object>)
            {
                CreateTreeViewItem(o, i);
            }
            else
            {
                item.Items.Add(o.GetType());
            }
            return item;
        }



        private bool MergeFrom(Type protoClass, string pathToProtos1, string pathToProtos2)
        {

            if (pathToProtos1 == "" || pathToProtos2 == "")
            {
                return false;
            }

            ConstructorInfo[] constructors = protoClass.GetConstructors();

            if (constructors.Count<ConstructorInfo>() > 0)
            {
                try
                {
                    var protoInstance1 = constructors[0].Invoke(null);
                    var protoInstance2 = constructors[0].Invoke(null);

                    IEnumerable<MethodInfo> protoMethods1 = (IEnumerable<MethodInfo>)protoInstance1.GetType().GetMethods();
                    IEnumerable<MethodInfo> protoMethods2 = (IEnumerable<MethodInfo>)protoInstance1.GetType().GetMethods();

                    var mergeFromList1 = protoMethods1.Where(m => m.Name == "MergeFrom").ToList();
                    var mergeFromList2 = protoMethods1.Where(m => m.Name == "MergeFrom").ToList();

                    var mergeFrom1 = mergeFromList1[1];
                    var mergeFrom2 = mergeFromList2[1];

                    byte[] protosBytes1 = File.ReadAllBytes(pathToProtos1);
                    var googleCode1 = new Google.Protobuf.CodedInputStream(protosBytes1);

                    byte[] protosBytes2 = File.ReadAllBytes(pathToProtos2);
                    var googleCode2 = new Google.Protobuf.CodedInputStream(protosBytes2);

                    mergeFrom1.Invoke(protoInstance1, new object[] { googleCode1 });
                    mergeFrom2.Invoke(protoInstance2, new object[] { googleCode2 });

                    this.proto1 = protoInstance1;
                    this.proto2 = protoInstance2;
                    return true;

                }

                catch (System.Reflection.TargetInvocationException i)
                {
                    return false;
                }
            }

            return false;

        }




        private void MergeAllFrom(List<Type> protoClasses, string pathToProtos1, string pathToProtos2)
        {

            if (pathToProtos1 == "" || pathToProtos2 == "")
            {
                MessageBox.Show("Please select data protos files first");
            }


            this.protoInstances1 = new List<object>();
            this.protoInstances2 = new List<object>();

            foreach (Type protoClass in protoClasses)
            {

                ConstructorInfo[] constructors = protoClass.GetConstructors();

                if (constructors.Count<ConstructorInfo>() > 0)
                {
                    try
                    {
                        var protoInstance1 = constructors[0].Invoke(null);
                        var protoInstance2 = constructors[0].Invoke(null);

                        IEnumerable<MethodInfo> protoMethods1 = (IEnumerable<MethodInfo>)protoInstance1.GetType().GetMethods();
                        IEnumerable<MethodInfo> protoMethods2 = (IEnumerable<MethodInfo>)protoInstance1.GetType().GetMethods();

                        var mergeFromList1 = protoMethods1.Where(m => m.Name == "MergeFrom").ToList();
                        var mergeFromList2 = protoMethods1.Where(m => m.Name == "MergeFrom").ToList();

                        var mergeFrom1 = mergeFromList1[1];
                        var mergeFrom2 = mergeFromList2[1];

                        byte[] protosBytes1 = File.ReadAllBytes(pathToProtos1);
                        var googleCode1 = new Google.Protobuf.CodedInputStream(protosBytes1);

                        byte[] protosBytes2 = File.ReadAllBytes(pathToProtos2);
                        var googleCode2 = new Google.Protobuf.CodedInputStream(protosBytes2);

                        mergeFrom1.Invoke(protoInstance1, new object[] { googleCode1 });
                        mergeFrom2.Invoke(protoInstance2, new object[] { googleCode2 });

                        protoInstances1.Add(protoInstance1);
                        protoInstances2.Add(protoInstance2);
                    }

                    catch (System.Reflection.TargetInvocationException i)
                    {

                    }
                }
            }
        }




        private List<Type> ListOfProtoClasses(string protoClassPath)
        {
            string[] classFilesPaths = System.IO.Directory.GetFiles(protoClassPath, "*.cs");

            string appPath = System.Environment.CurrentDirectory;
            System.IO.DirectoryInfo direct = new DirectoryInfo(appPath);

            var listofClasses = new List<Type>();

            var listofClassesForCombo = new List<string>();

            var listofProtosWithData = new List<Object>();

            foreach (var classPath in classFilesPaths)
            {
                foreach (var c in CompileProtoClass(classPath))
                {
                    var classname = (Type)c;
                    ConstructorInfo[] constructors = classname.GetConstructors();

                    if (constructors.Count<ConstructorInfo>() > 0)
                    {
                        listofClasses.Add(classname);
                        listofClassesForCombo.Add(classname.FullName);
                    }
                }
            }

            GenerateClassesButton.IsEnabled = false;
            return listofClasses;
        }


        private IList<Object> CompileProtoClass(string protoClassPath)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "Google.Protobuf.dll", "Google.ProtocolBuffers.dll" });
            parameters.GenerateInMemory = true;

            string classText = File.ReadAllText(protoClassPath, Encoding.UTF8);
            this.classContent = new string[] { classText };
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, this.classContent);

            if (results.Errors.HasErrors)
            {
                string message = "";
                results.Errors.Cast<CompilerError>().ToList().ForEach(er => message += er.ErrorText + " in line: " + er.Line + "\r\n");
                MessageBox.Show(message);
                return null;
            }
            else
            {
                var classes = results.CompiledAssembly.DefinedTypes;
                ICollection listOfClass = classes as ICollection;
                IList<Object> IclassList = (IList<Object>)classes;
                return IclassList;
            }

        }


        static string[] ReadClassFromFile()
        {
            string path = @"C:\Test1.txt";
            string[] readText = File.ReadAllLines(path, Encoding.UTF8);
            return readText;
        }


        private void OpenClass(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string readText = null;
            if (openFileDialog.ShowDialog() == true)
            {
                readText = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);
            }

            this.classContent = new string[] { readText };
        }



        private void ChooseProtoFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                labelReady.Content = "";
                this.protoPaths = new Dictionary<string, string>();
                foreach (var file in openFileDialog.FileNames)
                {
                    string protoPath = System.IO.Path.GetDirectoryName(file);
                    string protoName = System.IO.Path.GetFileName(file);
                    protoPaths.Add(protoName, protoPath);
                }
                buttonProtos1.IsEnabled = true;
                buttonProtos2.IsEnabled = true;
            }
        }


        private void GenerateProtoClasses(object sender, RoutedEventArgs e)
        {
            string appPath = System.Environment.CurrentDirectory;
            string pathToProtoexe = appPath + "\\protoc.exe";
            string classFolder = "\\" + Guid.NewGuid().ToString();
            string classPath = appPath + classFolder;
            System.IO.Directory.CreateDirectory(classPath);

            if (protoPaths != null)
            {
                foreach (var protoName in protoPaths.Keys)
                {
                    string args = "-I=" + protoPaths[protoName] + " --csharp_out=" + classPath + " " + protoPaths[protoName] + "\\" + protoName;
                    var protoExe = new Process();
                    protoExe.StartInfo.FileName = pathToProtoexe;
                    protoExe.StartInfo.Arguments = args;
                    protoExe.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    protoExe.Start();
                    protoExe.WaitForExit();
                }
            }
            else
            {
                comboBoxClasses.Height = 0;
                MessageBox.Show("Please select files first");
            }

            var listWithMarge = new List<Type>();

            foreach (Type protoClass in ListOfProtoClasses(classPath))
            {
                if (MergeFrom(protoClass, this.pathToProtos1, this.pathToProtos2))
                {
                    listWithMarge.Add(protoClass);
                }
            }


            this.listOfProtoClasses = new List<Type>(listWithMarge);

            var comboList = new List<string>();

            comboList.Add("Select all classes");
            foreach (Type t in this.listOfProtoClasses)
            {
                comboList.Add(t.ToString());
            }

            comboBoxClasses.SelectedItem = null;
            comboBoxClasses.Text = "Please Select Class";
            comboBoxClasses.Height = ((comboList.Count > 1) ? 27 : 0);
            labelReady.Foreground = ((comboList.Count > 1) ? Brushes.Green : Brushes.Red);
            labelReady.Content = ((comboList.Count > 1) ? "Classes ready!" : "Cannot generate classes or merge selected datas at any of the classes.");
            labelReady.FontSize = ((comboList.Count > 1) ? 14 : 12);

            if (comboList.Count > 1)
            {
                comboBoxClasses.ItemsSource = comboList;
            }

        }



        private void ChooseClass(object sender, SelectionChangedEventArgs e)
        {
            var select = comboBoxClasses.SelectedItem;

            if (sender != null)
            {
                if (select != null)
                {

                    if (select.ToString() == "Select all classes")
                    {
                        MergeAllFrom(this.listOfProtoClasses, this.pathToProtos1, this.pathToProtos2);
                        FillTreesWithObjects(this.protoInstances1, this.protoInstances2);
                    }
                    else
                    {
                        Type selectedClass = (Type)this.listOfProtoClasses.SingleOrDefault<Type>(t => t.ToString() == comboBoxClasses.SelectedItem.ToString());
                        var merge = MergeFrom(selectedClass, this.pathToProtos1, this.pathToProtos2);
                        FillTreesWithObjects(this.proto1, this.proto2);
                    }
                }
            }
        }

        private void OpenProtos1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.pathToProtos1 = openFileDialog.FileName;
                labelReady.Content = "";
                GenerateClassesButton.IsEnabled = true;
            }
        }

        private void OpenProtos2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                labelReady.Content = "";
                this.pathToProtos2 = openFileDialog.FileName;
                GenerateClassesButton.IsEnabled = true;
            }
        }

        public void GetCompositeObject(Composite parent, object obj, object nameOfProperty)
        {
            if (obj != null)
            {
                //////////////////////////////// FOR PROTOS OBJECTS ////////////////////////////////////////////////////////
                string typeofObject = obj.GetType().FullName;
                //  bool isParser = obj.GetType().Name == "Parser";
                //  bool isDescriptor = obj.GetType().Name == "Descriptor";
                bool isList = Regex.IsMatch(typeofObject, "Repeated") || Regex.IsMatch(typeofObject, "Generic.List");
                bool IsClass = !Regex.IsMatch(typeofObject, "System") && !Regex.IsMatch(typeofObject, "Repeated");
                bool isListOfTypes = Regex.IsMatch(typeofObject, "Generic.List");
                //////////////////////////////// FOR PROTOS OBJECTS ////////////////////////////////////////////////////////

                if (IsClass)
                {
                    ICollection properties = (ICollection)obj.GetType().GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        string propertyName = property.Name;
                        bool isNoParserOrdescriptor = !Regex.IsMatch(propertyName, "Parser") && !Regex.IsMatch(propertyName, "Descriptor");
                        if (isNoParserOrdescriptor)
                        {
                            object propertyValue = obj.GetType().GetProperty(propertyName).GetValue(obj, null);
                            Composite treeChild = new Composite(propertyName, propertyValue);
                            treeChild.ExpandedChanged += expand_Event;
                            // treeChild.parent = (Composite)obj;
                            parent.Add(treeChild);
                            GetCompositeObject(treeChild, propertyValue, propertyName);
                        }
                    }
                }

                if (isList)
                {
                    ICollection values = (ICollection)obj;
                    int i = 0;
                    foreach (object value in values)
                    {
                        object valueValue = values.GetType();
                        Composite treeChild = new Composite(nameOfProperty.ToString() + "[" + i + "]", value);
                        treeChild.ExpandedChanged += expand_Event;
                        parent.Add(treeChild);
                        GetCompositeObject(treeChild, value, nameOfProperty);
                        i++;
                    }
                }
            }

        }


        //public void GetCompositeObjectFromTestClass(Composite parent, object obj, object nameOfProperty)
        //{
        //    if (obj != null)
        //    {
        //        //////////////////////////////// FOR NORMAL OBJECTS ////////////////////////////////////////////////////////
        //        string typeofObject = obj.GetType().ToString();
        //        bool IsClass = !Regex.IsMatch(typeofObject, "System") && !Regex.IsMatch(typeofObject, "Generic.List");
        //        bool isList = Regex.IsMatch(typeofObject, "Generic.List");
        //        //////////////////////////////// FOR NORMAL OBJECTS ////////////////////////////////////////////////////////
        //        if (IsClass)
        //        {
        //            ICollection properties = (ICollection)obj.GetType().GetProperties();
        //            foreach (PropertyInfo property in properties)
        //            {
        //                string propertyName = property.Name;
        //                bool isNoParserOrdescriptor = !Regex.IsMatch(propertyName, "Parser") && !Regex.IsMatch(propertyName, "Descriptor");
        //                if (isNoParserOrdescriptor)
        //                {
        //                    object propertyValue = obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        //                    Composite treeChild = new Composite(propertyName, propertyValue);
        //                    //  treeChild.parent = obj as Composite;
        //                    treeChild.ExpandedChanged += expand_Event;
        //                    parent.Add(treeChild);
        //                    GetCompositeObjectFromTestClass(treeChild, propertyValue, propertyName);
        //                }
        //            }
        //        }

        //        if (isList)
        //        {
        //            ICollection values = (ICollection)obj;
        //            int i = 0;
        //            foreach (object value in values)
        //            {
        //                object valueValue = values.GetType();
        //                Composite treeChild = new Composite(nameOfProperty.ToString() + "[" + i + "]", value);
        //                treeChild.ExpandedChanged += expand_Event;
        //                parent.Add(treeChild);
        //                GetCompositeObjectFromTestClass(treeChild, value, nameOfProperty);
        //                i++;
        //            }
        //        }
        //    }

        //}
    }
}
