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
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Person player = new Person();
        List<Person> people = new List<Person>();
        Int32 id_counter = 0;

        public static string femaleNamesFile = "../../../names/first_name_female.txt";
        public static string maleNamesFile = "../../../names/first_name_male.txt";
        public static string surnamesFile = "../../../names/last_name.txt";

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        private FamilyWindow familyWindow = null;
        private ProfileWindow profileWindow = null;
        private Window overlayWindow = null;

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
        int TotalLines(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                int i = 0;
                while (r.ReadLine() != null) { i++; }
                return i;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            reset();
            birth();
            //pnlMainGrid.MouseUp += new MouseButtonEventHandler(pnlMainGrid_MouseUp);  - pozovi event
        }
        private void pnlMainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("You clicked me at " + e.GetPosition(this).ToString());
        }
        private void btnProgress_Click(object sender, RoutedEventArgs e)
        {
            event_renderer();
            age();
            lbResult.Items.Insert(0,player.age.ToString() + " years old:");

            //testing
            //database("list", player);
        }
        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            reset();
            lbResult.Items.Clear();
            birth();
        }
        private void btnFamily_Click(object sender, RoutedEventArgs e)
        {
                DialogHandler("family");
        }
        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
                DialogHandler("profile");
        }
        public void reset()
        {
            player.age = 0; ageLabel.Content = player.age;
            player.name = "default";
            player.surname = "default"; nameLabel.Content = player.name + " " + player.surname;
            player.id = 0;

            //empty the data
            clear();
        }
        public void birth()
        {
            //generate family
            generator("Father");
            generator("Mother");

            nameLabel.Content = player.name;
            ageLabel.Content = player.age;
            string namesFile = "";
            //give id
            player.id = id_counter;
            id_counter++;

            //assign a random gender
            if(RandomNumber(0,2) == 0) { player.gender = "male"; namesFile = maleNamesFile; }
            else { player.gender = "female"; namesFile = femaleNamesFile; }

            //give name
            var n1 = RandomNumber(0, TotalLines(namesFile));
            player.name = File.ReadLines(namesFile).ElementAt(n1); // 0-based

            //assign a surname equal to your father's
            player.surname = search("role", "Father").surname;
            nameLabel.Content = player.name + " " + player.surname;

            //add to database
            database("push", player);
            //introductory message
            if(player.gender == "male")
            {
                lbResult.Items.Insert(0, "You have been born a baby boy in U.S.A. Your parents decided to name you " + player.name + ".");
            }
            else
            {
                lbResult.Items.Insert(0, "You have been born a baby girl in U.S.A. Your parents decided to name you " + player.name + ".");
            }
            this.Title = "TinyLife Alpha 0.006 - " + player.name + " " + player.surname;
        }
        public void age()
        {
            foreach(Person person in people)
            {
                person.age++;
            }
            ageLabel.Content = player.age;
        }
        public void event_renderer()
        {
            lbResult.Items.Insert(0, "Your ID is: " + player.id.ToString());
        }
        public void event_handler()
        {

        }
        public void generator(String arg)
        {
            Person person = new Person();
            String namesFile = "";
            //give id
            person.id = id_counter;
            id_counter++;

            switch (arg)
            {
                case "Mother":
                case "Female_character":
                case "Sister":
                    person.gender = "female";
                    namesFile = femaleNamesFile;
                    break;
                case "Father":
                case "Male_character":
                case "Brother":
                    person.gender = "male";
                    namesFile = maleNamesFile;
                    break;
                case "Sibling":
                    randomGender();
                    break;
            }

            //assign a random gender if called
            void randomGender()
            {
                //assign a random gender
                if (RandomNumber(0, 1) == 0) { person.gender = "male"; namesFile = maleNamesFile; }
                else { person.gender = "female"; namesFile = femaleNamesFile; }
            }

            //give tags equal to arguments
            if (arg != "null")
            {
                person.pTag.role.Add(arg);
                if (arg == "Father" || arg == "Mother" || arg == "Sibling")
                {
                    person.isFamily = true;
                }
            }

            //give name
            var n1 = RandomNumber(0, TotalLines(namesFile));
            person.name = File.ReadLines(namesFile).ElementAt(n1); // 0-based

            //give surname (if family, assign yours)
            if (person.isFamily == true && arg != "Father")
            {
                person.surname = search("role", "Father").surname;
            }
            else
            {
                var n2 = RandomNumber(0, TotalLines(surnamesFile));
                person.surname = File.ReadLines(surnamesFile).ElementAt(n2);
            }

            //give proper age depending on role
            person.age = int.Parse(designator("age", arg));

            //add to database
            database("push", person);

            string designator(string type, string role)
            {
                switch (type)
                {
                    case "age":
                        switch (role)
                        {
                            case "Father":
                            case "Mother":
                                return RandomNumber(16, 35).ToString();
                            case "Sibling":
                                return RandomNumber(1, 10).ToString();
                        }
                        break;
                }
                return "0";
            }

        }
        public void database(String arg, Person person)
        {
            switch(arg)
            {
                case "push":
                    push(person);
                    break;
                case "list":
                    foreach (Person per in people)
                    {
                        MessageBox.Show(per.name.ToString());
                        //writes down all the persons roles
                        foreach (string o in per.pTag.role)
                        {
                            if(per.pTag.role.Count > 0)
                            {
                                MessageBox.Show(o);
                            }
                        }
                    }
                    break;
            }
            void push(Person received)
            {
                people.Add(received);
            }
        }
        public Person search(string type, string arg1)
        {
            switch(type)
            {
                //search for a specific person by role
                case "role":
                    foreach (Person per in people)
                    {
                        //writes down all the persons roles
                        foreach (string o in per.pTag.role)
                        {
                            if (o == arg1)
                            {
                                return per;
                            }
                        }
                    }
                    break;
                //search for a specific person by id
                case "id":
                    foreach (Person per in people)
                    {
                        //writes down all the persons roles
                        if (per.id == int.Parse(arg1))
                        {
                                return per;
                        }
                    }
                    break;
            }
            return null;
        }
        public void clear()
        {
            people.Clear();
            id_counter = 0;
        }
        public void DialogHandler(string arg)
        {
            if (overlayWindow == null)
            {
                switch (arg)
                {
                    case "family":
                        familyWindow = new FamilyWindow(people);
                        familyWindow.Closed += OverlayWindowClosed;
                        //ShowDialog() makes the window stay in focus until dissmissed
                        familyWindow.ShowDialog();
                        break;
                    case "profile":
                        //change player for a search
                        profileWindow = new ProfileWindow(player);
                        profileWindow.Closed += OverlayWindowClosed;
                        profileWindow.ShowDialog();
                        break;
                }
            }
        }
        public void OverlayWindowClosed(object sender, System.EventArgs e)
        {
            overlayWindow = null;
        }
    }

    public class Player
    {

    }

    public class Person
    {
        //list of personal defined tags
        public tags pTag = new tags();
        public class tags
        {
            public List<string> role { get; set; } = new List<string>();
        }
        public bool isFamily { get; set; } = false;
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public int age { get; set; }
        public string gender { get; set; }
    }
}
