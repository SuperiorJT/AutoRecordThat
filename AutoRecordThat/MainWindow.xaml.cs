using System;
using System.Collections.Generic;
using System.Linq;
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

namespace AutoRecordThat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<string, IntPtr> windowMap = new Dictionary<string, IntPtr>();
        public List<string> WindowTitles = new List<string>();

        IntPtr selectedWindow = new IntPtr();
        string selectedWindowTitle = "";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            UpdateWindowList();
            cbWindowTitles.SelectedIndex = 0;
            selectedWindowTitle = WindowTitles[0];
            windowMap.TryGetValue(WindowTitles[0], out selectedWindow);
        }

        private void UpdateWindowList()
        {
            List<IntPtr> windows = WindowManager.FilterWindows();
            windows.ForEach(AddWindowToMap);
            WindowTitles = windowMap.Keys.ToList();
            cbWindowTitles.ItemsSource = WindowTitles;
            if (selectedWindowTitle != "")
            {
                windowMap.TryGetValue(selectedWindowTitle, out selectedWindow);
            }
        }

        private void AddWindowToMap(IntPtr hWnd)
        {
            int size = WindowManager.GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(size++);
            WindowManager.GetWindowText(hWnd, sb, size);
            int count = 0;
            string title = "[" + count + "] - " + sb.ToString();
            bool isInMap = windowMap.ContainsKey(title);
            while (isInMap)
            {
                count++;
                title = "[" + count + "] - " + sb.ToString();
                isInMap = windowMap.ContainsKey(title);
            }
            windowMap.Add(title, hWnd);
        }

        void UpdateSelectedWindow(object sender, SelectionChangedEventArgs args)
        {
            string key = (string) cbWindowTitles.SelectedValue;
            selectedWindowTitle = key;
            windowMap.TryGetValue(key, out selectedWindow);
        }
    }
}
