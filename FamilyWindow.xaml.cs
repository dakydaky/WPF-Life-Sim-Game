using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for FamilyWindow.xaml
    /// </summary>
    public partial class FamilyWindow : Window
    {
        List<Person> people = new List<Person>();
        private ProfileWindow profileWindow = null;
        public FamilyWindow(List<Person> sent)
        {
            InitializeComponent();
            FillData(sent);
            //starts the window in the exact center of the parent
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
        }
        public void FillData(List<Person> personList)
        {
            people = personList;

            //populate a list with family members
            List<Person> family = people.FindAll(x => x.isFamily == true);
            foreach (Person person in family)
            {
                //assigns roles to family members
                string roles = "";
                foreach(String role in person.pTag.role)
                {
                    roles += role + ", ";
                }
                //creates the card with personal data
                Button btn = new Button
                {
                    Name = person.name,
                    Tag = person.id.ToString(),
                    Content = roles + person.name + " " + person.surname + ", age " + person.age,
                    Height = 152,
                };
                btn.Click += (sender, EventArgs) => { btnMember_Click(sender, EventArgs, btn.Tag.ToString()); };

                familyList.Children.Add(btn);
            }
        }
        private void btnMember_Click(object sender, RoutedEventArgs e, string id)
        {
            if(profileWindow == null)
            {
                foreach (Person per in people)
                {
                    //writes down all the persons roles
                    if (per.id == int.Parse(id))
                    {
                        profileWindow = new ProfileWindow(per);
                        profileWindow.Closed += ProfileWindowClosed;
                        profileWindow.ShowDialog();
                    }
                }
            }
        }
        public void ProfileWindowClosed(object sender, System.EventArgs e)
        {
            profileWindow = null;
        }
    }
}
