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

using Newtonsoft.Json;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    



    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            jira = new JiraConnector("jira.550550.ru", ProtocolType.HTTPS);
            
            InitializeComponent();
        }

        private JiraConnector jira;
        
        private Grid GenerateIssueItem(int i, SearchIssues.RootSearchIssueObject issuesObject)
        {
            var issue = issuesObject.issues[i];
            var tag = issue.key;
            var summary = issue.fields.summary;
            var desc = issue.fields.description;
            var updated = issue.fields.updated;
            var created = issue.fields.created;
            var agent = issue.fields.assignee.name;
            Border issueInfoGridBorder = new Border() { BorderBrush = Brushes.Silver, BorderThickness = new Thickness(1) };
            Grid issueInfoGrid = new Grid() { Width = myTasks.Width, HorizontalAlignment = HorizontalAlignment.Center, Name = "IssueInfoGrid", };
            RowDefinition row1 = new RowDefinition() { Height = new GridLength(30) };
            RowDefinition row2 = new RowDefinition() { Height = new GridLength(30) };
            RowDefinition row3 = new RowDefinition() { Height = new GridLength(30) };
            RowDefinition row4 = new RowDefinition() { Height = new GridLength(30) };
            ColumnDefinition col1 = new ColumnDefinition() { Width = new GridLength(200) };
            ColumnDefinition col2 = new ColumnDefinition() { Width = new GridLength(500) };
            ColumnDefinition col3 = new ColumnDefinition() { Width = new GridLength(100) };
            issueInfoGrid.RowDefinitions.Add(row1);
            issueInfoGrid.RowDefinitions.Add(row2);
            issueInfoGrid.RowDefinitions.Add(row3);
            issueInfoGrid.RowDefinitions.Add(row4);
            issueInfoGrid.ColumnDefinitions.Add(col1);
            issueInfoGrid.ColumnDefinitions.Add(col2);
            issueInfoGrid.ColumnDefinitions.Add(col3);

            TextBlock issueHeader = new TextBlock() { Text = summary, Width = 200, TextWrapping = TextWrapping.Wrap };
            TextBlock issueDesc = new TextBlock() { Text = desc, Width = issueInfoGrid.Width - 400, TextWrapping = TextWrapping.Wrap };
            TextBlock issueCreated = new TextBlock() { Text = "created", Width = 200, TextWrapping = TextWrapping.Wrap };
            TextBlock issueUpdated = new TextBlock() { Text = "updated", Width = 200, TextWrapping = TextWrapping.Wrap };
            TextBlock issueAgent = new TextBlock() { Text = agent, Width = 200, TextWrapping = TextWrapping.Wrap };

            issueInfoGrid.Children.Add(issueHeader);
            issueInfoGrid.Children.Add(issueDesc);
            issueInfoGrid.Children.Add(issueCreated);
            issueInfoGrid.Children.Add(issueUpdated);
            issueInfoGrid.Children.Add(issueAgent);

            Grid.SetRow(issueHeader, 0);
            Grid.SetRow(issueAgent, 0);
            Grid.SetRow(issueDesc, 1);
            Grid.SetRow(issueCreated, 2);
            Grid.SetRow(issueUpdated, 3);

            Grid.SetColumn(issueHeader, 0);
            Grid.SetColumn(issueCreated, 2);
            Grid.SetColumn(issueDesc, 1);
            Grid.SetColumn(issueAgent, 2);
            Grid.SetColumn(issueUpdated, 2);

            return issueInfoGrid;
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            jira.Auth(LoginInput.Text, PwdInput.Password);
            debugText.TextWrapping = TextWrapping.Wrap;
            debugText.Text = jira.DebugText;
            loginGrid.Visibility = Visibility.Hidden;
            generalTabControl.Visibility = Visibility.Visible;
            
        }

        private void GeneralTabControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (generalTabControl.Visibility == Visibility.Visible)
                {
                    generalTabControl.SelectedItem = myTasks;
                }
            }
            catch (Exception exc)
            {
                debugText.TextWrapping = TextWrapping.Wrap;
                debugText.Text = "GeneralTabControl::Esception"+exc.Message;
            }
        }

        private void MyTasks_Selected(object sender, RoutedEventArgs e)
        {
            string issueObjectText = "";
            try
            {
                if (generalTabControl.Visibility == Visibility.Visible)
                {
                    issueObjectText = jira.getIssues(SearchType.myIssues);
                    if (issueObjectText.Contains("->"))
                    {
                        debugText.Text = "MyTasks Exception -> "+issueObjectText;
                    }
                    else
                    {
                        SearchIssues.RootSearchIssueObject issueObject = JsonConvert.DeserializeObject<SearchIssues.RootSearchIssueObject>(issueObjectText);
                        ListBox issueList = new ListBox() { };
                        for (var i = 0; i < issueObject.maxResults; i++)
                        {
                            var issueItem = GenerateIssueItem(i, issueObject);
                            issueList.Items.Add(issueItem);
                        }
                        myTasks.Content = issueList;
                    }
                }
            }
            catch (Exception exc)
            {
                debugText.TextWrapping = TextWrapping.Wrap;
                debugText.Text += "GeneralTabControl::SelectedEsception ->" + exc.Message + issueObjectText;
            }
        }

        private void CallCenter_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (generalTabControl.Visibility == Visibility.Visible)
                {
                    var issueObjectText = jira.getIssues(SearchType.callCenter);
                    if (issueObjectText.Contains("->"))
                    {
                        debugText.Text = "CallCenter Exception -> " + issueObjectText;
                    }
                    else
                    {
                        SearchIssues.RootSearchIssueObject issueObject = JsonConvert.DeserializeObject<SearchIssues.RootSearchIssueObject>(issueObjectText);
                        ListBox issueList = new ListBox() { };
                        for (var i = 0; i < issueObject.maxResults; i++)
                        {
                            var issueItem = GenerateIssueItem(i, issueObject);
                            issueList.Items.Add(issueItem);
                        }
                        myTasks.Content = issueList;
                    }
                }
            }
            catch (Exception exc)
            {
                debugText.TextWrapping = TextWrapping.Wrap;
                debugText.Text = "GeneralTabControl::Esception" + exc.Message;
            }
        }

        private void HotLine_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (generalTabControl.Visibility == Visibility.Visible)
                {
                    var issueObjectText = jira.getIssues(SearchType.hotLine);
                    if (issueObjectText.Contains("->"))
                    {
                        debugText.Text = "HotLine Exception -> " + issueObjectText;
                    }
                    else
                    {
                        SearchIssues.RootSearchIssueObject issueObject = JsonConvert.DeserializeObject<SearchIssues.RootSearchIssueObject>(issueObjectText);
                        ListBox issueList = new ListBox() { };
                        for (var i = 0; i < issueObject.maxResults; i++)
                        {
                            var issueItem = GenerateIssueItem(i, issueObject);
                            issueList.Items.Add(issueItem);
                        }
                        myTasks.Content = issueList;
                    }
                }
            }
            catch (Exception exc)
            {
                debugText.TextWrapping = TextWrapping.Wrap;
                debugText.Text = "GeneralTabControl::Esception" + exc.Message;
            }
        }

        private void VoiceMail_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (generalTabControl.Visibility == Visibility.Visible)
                {
                    var issueObjectText = jira.getIssues(SearchType.voiceMail);
                    if (issueObjectText.Contains("->"))
                    {
                        debugText.Text = "VoiceMail Exception -> " + issueObjectText;
                    }
                    else
                    {
                        SearchIssues.RootSearchIssueObject issueObject = JsonConvert.DeserializeObject<SearchIssues.RootSearchIssueObject>(issueObjectText);
                        ListBox issueList = new ListBox() { };
                        for (var i = 0; i < issueObject.maxResults; i++)
                        {
                            var issueItem = GenerateIssueItem(i, issueObject);
                            issueList.Items.Add(issueItem);
                        }
                        myTasks.Content = issueList;
                    }
                }
            }
            catch (Exception exc)
            {
                debugText.TextWrapping = TextWrapping.Wrap;
                debugText.Text = "GeneralTabControl::Esception" + exc.Message;
            }
        }
    }
}
