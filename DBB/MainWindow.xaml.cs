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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data;
using DBB.Cls;
using DBB.Repo;

namespace DBB
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        DBList dbRepo = new DBList();
        DTList dtRepo = new DTList();
        BackUp backup = new BackUp();
        int dataCnt = 0;
        int buttonWidth = 400;
        int buttonHeight = 50;
        public MainWindow()
        {
            InitializeComponent();
            DBComboList.ItemsSource = dbRepo.dbList;
            StartHourList.ItemsSource = dtRepo.hourList;
            StartMinList.ItemsSource = dtRepo.minList;
            LastHourList.ItemsSource = dtRepo.hourList;
            LastMinList.ItemsSource = dtRepo.minList;
            DBComboList.SelectedIndex = StartHourList.SelectedIndex = StartMinList.SelectedIndex = LastHourList.SelectedIndex = LastMinList.SelectedIndex = 0;
        }

        private async void loadBtn_Click(object sender, EventArgs e)
        {
            if (DBComboList.Text.Equals("Select DB"))
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("유효한 DB를 선택해주세요.");
                return;
            }
            status.Text = "loading...";
            dtRepo.loadTime.Add("db", DBComboList.Text);
            dtRepo.loadTime.Add("startDay", StartDay.Text);
            dtRepo.loadTime.Add("startHour", StartHourList.Text);
            dtRepo.loadTime.Add("startMin", StartMinList.Text);
            dtRepo.loadTime.Add("lastHour", LastHourList.Text);
            dtRepo.loadTime.Add("lastMin", LastMinList.Text);
            string content = DBComboList.Text + ") " + StartDay.Text + " " + StartHourList.Text + ":" + StartMinList.Text +
                " ~ " + StartDay.Text + " " + LastHourList.Text + ":" + LastMinList.Text;
            await Task.Run(() =>
            {
                dataCnt = backup.LoadDataLog(dtRepo.loadTime);
                Application.Current.Dispatcher.Invoke(() => UpdateUI(content));
                dtRepo.loadTime.Clear();
            });
        }
        private void UpdateUI(string content)
        {

            Button dataBtn = new Button();
            dataBtn.Content = content;
            dataBtn.DataContext = dataCnt;
            dataBtn.Width = buttonWidth;
            dataBtn.Height = buttonHeight;
            dataBtn.Click += Button_Click;
            DataGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(buttonHeight + 10) });
            DataGrid.Children.Add(dataBtn);
            Grid.SetRow(dataBtn, DataGrid.RowDefinitions.Count - 1);
            MessageBox.Show("ok");
            status.Text = "Idle";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("BackUp을 진행하시겠습니까?", "확인", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                int cnt = 0;
                var btn = e.Source as Button;
                int dataIdx = Convert.ToInt32(btn.DataContext);
                await Task.Run(() => cnt = backup.saveDataLog(dataIdx));
                if(cnt == -1)
                {
                    MessageBox.Show("BackUp 실패");
                    return;
                }
                MessageBox.Show("BackUp 완료 | Fail Count : " + cnt);
            }
        }


    }
}
