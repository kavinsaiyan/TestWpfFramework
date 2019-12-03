using System;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using JapTestAppLogic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squirrel;

namespace Test_WPF_Framework
{
    /// <summary>
    /// Interaction logic for Test1.xaml
    /// </summary>
    public partial class Test1 : Window
    {
        public Test1()
        {
            InitializeComponent();
            Task.Run(async () => { await CheckForUpdates(); });
            SetListBoxData();
        }

        private List<TestPortion> testPortions = new List<TestPortion>();

        private int NoOfQuestions = 20;

        //constants for no of questions
        private readonly int MaxNoOfQuestions = 100;
        private readonly int MinNoOfQuestions = 10;

        public int NoOfQuestonsProperty
        {
            get { return NoOfQuestions; }
            set
            {
                if (value <= MinNoOfQuestions)
                    NoOfQuestions = MinNoOfQuestions;
                else if (value >= MaxNoOfQuestions)
                    NoOfQuestions = MaxNoOfQuestions;
                else
                    NoOfQuestions = value;
            }
        }

        private int PartitionNo = 5;

        private int currentIndex = 0;
        public int CurrentQuestionIndex
        {
            get { return currentIndex; }
            set
            {
                if (value <= 0) currentIndex = 0;
                else if (value >= NoOfQuestonsProperty) currentIndex = NoOfQuestonsProperty - 1;
                else currentIndex = value;
            }
        }

        //methods

        private async Task CheckForUpdates()
        {
            Task<UpdateManager> manager;
            //MyUtility.WriteLineMessageBox("Success!!!!?");
            using (manager = UpdateManager.GitHubUpdateManager(@"https://github.com/kavinsaiyan/TestWpfFramework"))
            {
                //MyUtility.WriteLineMessageBox($"Status?:{manager.Status}\n v:{System.Net.ServicePointManager.SecurityProtocol}");
                //try {
                //    UpdateInfo updateInfo =  await manager.Result.CheckForUpdate();
                //    //MyUtility.WriteLineMessageBox($"{updateInfo.ReleasesToApply.Count}");
                //    //MyUtility.WriteLineMessageBox($"Status?:{manager.Result.CheckForUpdate().Exception.InnerExceptions[2].Message}");
                //}
                //catch (Exception e) { 
                //    MyUtility.WriteLineMessageBox(e.InnerException.ToString());
                //    MyUtility.WriteLineDebug(e.InnerException.ToString());
                //}
                var val = await manager.Result.UpdateApp();
                // MyUtility.WriteLineMessageBox("Successfully updated?");
            }
            manager.Result.Dispose();
            manager = null;
            GC.WaitForFullGCComplete();

        }

        private void SetListBoxData()
        {
            //JPLT_N5_Vocabulary testportions
            List<TestPortion> list = new List<TestPortion>();
            TestPortion test = new TestPortion("JPLT_N5_Vocabulary", "DataBase.sdf", "JapaneseData", "JPLT_N5_Vocabulary ", 716);
            list.Add(test);

            //JPLT_N4_Vocabulary
            test = new TestPortion("JPLT_N4_Vocabulary", "DataBase.sdf", "JapaneseData", "JPLT_N4_Vocabulary ", 716);
            list.Add(test);

            TestPortionsListBox.ItemsSource = list;
        }

        private static bool IsTextAllowed(string text)
        {
            return !(new Regex("[^0-9]+")).IsMatch(text);
        }

        private void SetQuestionsAndAnswers()
        {
            int reqIndex = CurrentQuestionIndex / PartitionNo;
            int count = testPortions[reqIndex].questionAndAnswersList.Count;
            QuestionAndAnswer temp = testPortions[reqIndex].questionAndAnswersList[CurrentQuestionIndex % count];

            Question.Text = temp.Question;

            //clearing the checked Answers
            AnswerButton0.IsChecked = false;
            AnswerButton1.IsChecked = false;
            AnswerButton2.IsChecked = false;
            AnswerButton3.IsChecked = false;

            AnswerButton0.Content = temp.AnswerOptions[0];
            AnswerButton1.Content = temp.AnswerOptions[1];
            AnswerButton2.Content = temp.AnswerOptions[2];
            AnswerButton3.Content = temp.AnswerOptions[3];
        }

        private void GeneratePartitions(int noOfTestPortions)
        {
            PartitionNo = NoOfQuestonsProperty / noOfTestPortions;
        }

        private void SetScoreAndAnswers()
        {
            int score = 0;
            string answer = string.Empty;
            int index = 1;
            //calculating score
            foreach (var i in testPortions)
                foreach (var q in i.questionAndAnswersList)
                {
                    if (q.CheckIfCorrectLock())
                        score++;

                    //find locked answer
                    string yourAnswer = string.Empty;
                    if (q.GetLock() >= 0 && q.GetLock() <= 3)
                        yourAnswer = q.AnswerOptions[q.GetLock()];

                    //set the answer string
                    answer += $"{ index++ }.{q.Question}:{ q.GetCorrectAnswer() } \tYour Answer:{ yourAnswer }{Environment.NewLine}";
                }

            //setting the score
            YourScoreTextBlock.Text += $"{score}/{NoOfQuestions}";

            //setting the answers
            AnswersTextBlock.Text = answer;
        }

        //events for taskBar buttons
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                bitmap.UriSource = new Uri(@"pack://application:,,,/Test WPF Framework;component/Resources/MaximizeJapanese.png");
            }
            else
            {
                WindowState = WindowState.Maximized;
                bitmap.UriSource = new Uri(@"pack://application:,,,/Test WPF Framework;component/Resources/RestoreJapanese.png");
            }
            bitmap.EndInit();
            MaximizeOrRestoreImage.Source = bitmap;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            //adding the selected testPortions
            foreach (var item in TestPortionsListBox.SelectedItems)
            {
                testPortions.Add(item as TestPortion);
            }

            if (testPortions.Count == 0)
            {
                MessageBox.Show("続行する前に部分を選択してください!!");
                return;
            }

            //check the no of questions
            int n;
            if (int.TryParse(NoOfQuestionsTextBox.Text.Trim(), out n))
            {
                if (MinNoOfQuestions > n)
                {
                    MyUtility.WriteLineMessageBox("少なくとも10の質問はありません！");
                    NoOfQuestionsTextBox.Text = MinNoOfQuestions.ToString();
                    return;
                }
                if (MaxNoOfQuestions < n)
                {
                    MyUtility.WriteLineMessageBox("質問はせいぜい100でなければなりません！");
                    NoOfQuestionsTextBox.Text = MaxNoOfQuestions.ToString();
                    return;
                }
                //set the no of questions
                NoOfQuestions = n;
            }

            //disabling and enabling the panels
            StartingPanel.Visibility = Visibility.Hidden;
            StartingPanel.IsEnabled = false;
            SetQuestionsPanel.Visibility = Visibility.Visible;
            SetQuestionsPanel.IsEnabled = true;

            ChooseQuestionsListBox.ItemsSource = testPortions;

            //generate partitions
            GeneratePartitions(testPortions.Count);
        }

        private async void TakeTestButton_Click(object sender, RoutedEventArgs e)
        {
            //update the testportions list
            testPortions = ChooseQuestionsListBox.ItemsSource as List<TestPortion>;

            foreach (TestPortion testPortion in testPortions)
            {
                await testPortion.GenerateQuestionsParallelAsync(PartitionNo, testPortion.StartingIndexProperty, testPortion.EndingIndexProperty);
            }

            //enable or disable the panels
            QuestionAnswerPanel.Visibility = Visibility.Visible;
            QuestionAnswerPanel.IsEnabled = true;
            SetQuestionsPanel.Visibility = Visibility.Hidden;
            SetQuestionsPanel.IsEnabled = false;

            //Set Questions
            SetQuestionsAndAnswers();
        }

        //this is done in order to prevent alphabetic input in the TextBoxes
        private void AllowNumbersOnlyEvent(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void IndexTextBlock_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void StartIndexTextBlock_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ChooseQuestionsListBox.SelectedItem == null)
            {
                StartIndexTextBlock.Text = string.Empty;
                MyUtility.WriteLineMessageBox("インデックスを与える前に部分を選択してください!!!!");
                return;
            }
            var temp = ChooseQuestionsListBox.SelectedItem as TestPortion;
            string s = StartIndexTextBlock.Text.Trim();
            if (s != string.Empty)
                temp.StartingIndexProperty = int.Parse(StartIndexTextBlock.Text.Trim());
        }

        private void EndIndexTextBlock_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ChooseQuestionsListBox.SelectedItem == null)
            {
                StartIndexTextBlock.Text = string.Empty;
                MyUtility.WriteLineMessageBox("インデックスを与える前に部分を選択してください!!!!");
                return;
            }
            var temp = ChooseQuestionsListBox.SelectedItem as TestPortion;
            string s = EndIndexTextBlock.Text.Trim();
            if (s != string.Empty)
                temp.EndingIndexProperty = int.Parse(EndIndexTextBlock.Text.Trim());
        }

        private void ChooseQuestionsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var temp = ChooseQuestionsListBox.SelectedItem as TestPortion;
            StartIndexTextBlock.Text = temp.StartingIndexProperty.ToString();
            EndIndexTextBlock.Text = temp.EndingIndexProperty.ToString();
        }

        private void PreviousQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentQuestionIndex--;
            SetQuestionsAndAnswers();
        }

        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentQuestionIndex++;
            SetQuestionsAndAnswers();
            if (currentIndex == NoOfQuestions - 1)
                SubmitTestButton.IsEnabled = true;
        }

        private void SubmitTestButton_Click(object sender, RoutedEventArgs e)
        {
            //enable and disable panels
            QuestionAnswerPanel.Visibility = Visibility.Hidden;
            QuestionAnswerPanel.IsEnabled = false;
            ShowAnswersPanel.Visibility = Visibility.Visible;
            ShowAnswersPanel.IsEnabled = true;

            //set Score and Answers 
            SetScoreAndAnswers();
        }

        private void AnswerButton0_Checked(object sender, RoutedEventArgs e)
        {
            int reqIndex = CurrentQuestionIndex / PartitionNo;
            int count = testPortions[reqIndex].questionAndAnswersList.Count;
            testPortions[reqIndex].questionAndAnswersList[CurrentQuestionIndex % count].setLock(0);
        }

        private void AnswerButton1_Checked(object sender, RoutedEventArgs e)
        {
            int reqIndex = CurrentQuestionIndex / PartitionNo;
            int count = testPortions[reqIndex].questionAndAnswersList.Count;
            testPortions[reqIndex].questionAndAnswersList[CurrentQuestionIndex % count].setLock(1);
        }

        private void AnswerButton2_Checked(object sender, RoutedEventArgs e)
        {
            int reqIndex = CurrentQuestionIndex / PartitionNo;
            int count = testPortions[reqIndex].questionAndAnswersList.Count;
            testPortions[reqIndex].questionAndAnswersList[CurrentQuestionIndex % count].setLock(2);
        }

        private void AnswerButton3_Checked(object sender, RoutedEventArgs e)
        {
            int reqIndex = CurrentQuestionIndex / PartitionNo;
            int count = testPortions[reqIndex].questionAndAnswersList.Count;
            testPortions[reqIndex].questionAndAnswersList[CurrentQuestionIndex % count].setLock(3);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;
            this.DragMove();
        }
    }
}
