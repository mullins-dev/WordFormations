using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace WordFormations
{
    class MainPage : ContentPage
    {
        //================================================================================================================= Properties
        private EnglishDictionary servicer;
        private ISet<string> availableWords;

        private String allAvailableLetters;

        private StackLayout master;

        // Top level visuals/stats
        private StackLayout topRow;
        private Label numWords;
        private Label wordBuilder;
        private int totWords; // Count of how many words are in the availableWords HashSet
        private int numFormed; // Running number of words the user has formed

        // Button things
        private ButtonID[] buttonArray;
        private StackLayout buttonContainer;

        // Functionality buttons (i.e. Done, New Game)
        private StackLayout controlPanel;
        private Button done;
        private Button backspace;
        private Button fill;
        private Button newGame;

        // ListView with completed words
        private StackLayout completedWords;
        private ListView lps;
        private ObservableCollection<string> doneWords;

        // Keeps track of last button clicked.
        private Stack<int> lastClickedButtons;

        //================================================================================================================= Constructor
        public MainPage()
        {
            servicer = new EnglishDictionary("words.txt");
            master = new StackLayout { };
            AddHeader();
            WordScoreLabel();
            master.Children.Add(topRow);
            buttonContainer = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.Center };
            AddButtons();
            master.Children.Add(buttonContainer);
            AddControls();
            master.Children.Add(controlPanel);
            master.Children.Add(new Label { Text = "Formed Words:", FontFamily = "Helvetica", FontAttributes = FontAttributes.Italic, FontSize = 25, TextColor = Color.Black, Padding = new Thickness(5, 0) });
            AddListView();
            master.Children.Add(completedWords);
            GetAvailableWords();
            this.Content = master;
            this.Content.BackgroundColor = Color.Khaki;
        }

        //================================================================================================================= On-Clicked/Events

        /*
         * Adds a letter to the word-formation label when a letter button is clicked. 
         */
        private void LetterClicked(object sender, EventArgs e)
        {
            ButtonID b = (ButtonID)sender;
            string letter = b.Text;
            lastClickedButtons.Push(b.IDNum);
            b.IsEnabled = false;
            wordBuilder.Text += letter;
        }

        /**
         * Removes a letter from the word being formed upon the "backspace" button being clicked.
         * If there are no letters to remove, the event will not work.
         */
        private void BackSpaceClicked(object sender, EventArgs e)
        {
            if (wordBuilder.Text.Length > 0)
            {
                wordBuilder.Text = wordBuilder.Text.Substring(0, wordBuilder.Text.Length - 1);
                FindEnableButton();
            }
        }

        /*
         * Checks if the given word exists within the possible words to be created
         */
        private void DoneClicked(object sender, EventArgs e)
        {
            if (numWords.Text.Equals(""))
            {
                numWords.Text = "0/" + totWords;
            }
            if (availableWords.Contains(wordBuilder.Text) && !doneWords.Contains(wordBuilder.Text))
            {
                doneWords.Add(wordBuilder.Text);
                wordBuilder.Text = "";
                numWords.Text = "";
                numFormed++;
                numWords.Text = numFormed + "/" + totWords;
                foreach (var item in buttonArray)
                {
                    item.IsEnabled = true;
                }
                lastClickedButtons.Clear();
            }
            CheckCompletedGame();
        }


        //================================================================================================================= Methods

        /*
         * Checks if the game is over. If so, all buttons are disabled and "Game Complete!" shows up where the words are formed
         */
        private void CheckCompletedGame()
        {
            if (numFormed == availableWords.Count) // If user has formed a number of words equal to that of the available words
            {
                foreach (ButtonID bt in buttonArray)
                {
                    bt.IsEnabled = false;
                }
                wordBuilder.Text = "Game Complete!";
                done.IsEnabled = false;
                backspace.IsEnabled = false;
                fill.IsEnabled = false;
            }
        }

        /*
         * Basic header which boosts the appearance of the game
         */
        public void AddHeader()
        {
            StackLayout stk = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.Center };
            Label header = new Label { Text = "Word Formations!", FontFamily = "Times New Roman", FontSize = 30, Padding = new Thickness(0, 15, 0, 15) };
            stk.Children.Add(header);
            master.Children.Add(stk);
        }

        /**
          * Obtains the available words that are creatable with the given set of letters.
           */
        private void GetAvailableWords()
        {
            availableWords = new HashSet<string>();
            availableWords = servicer.ScrabbleWords(allAvailableLetters, 0);
            totWords = availableWords.Count;

        }

        private void WordScoreLabel()
        {
            topRow = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 20 };
            numWords = new Label { Text = "", FontSize = 35, TextColor = Color.Black, Padding = new Thickness(10, 0, 0, 0) };
            wordBuilder = new Label { Text = "", FontSize = 20, Margin = new Thickness(20), TextColor = Color.Black };
            topRow.Children.Add(numWords);
            topRow.Children.Add(wordBuilder);
            numFormed = 0;
        }

        /*
         * Adds the randomly-generated letters into the window
         */
        private void AddButtons()
        {
            char[] currentLetters = GenerateRandomLetters("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            buttonArray = new ButtonID[7];
            // Set 'allAvailableLetters' to an empty string. This will keep track of all our letters we have on our buttons
            allAvailableLetters = "";
            int i = 0;
            while (i < currentLetters.Length)
            {
                // Button b = new Button { Text = "" + currentLetters[i], WidthRequest = 50, HeightRequest = 50 };
                ButtonID b = new ButtonID
                {
                    Text = "" + currentLetters[i],
                    WidthRequest = 50,
                    HeightRequest = 50,
                    IDNum = i,
                    BackgroundColor = Color.Beige,
                    BorderColor = Color.Black,
                    BorderWidth = 1,
                    CornerRadius = 5
                };
                b.Clicked += LetterClicked;
                buttonContainer.Children.Add(b);
                allAvailableLetters += "" + currentLetters[i];
                buttonArray[i] = b;
                i++;
            }
            lastClickedButtons = new Stack<int>(); // Instantiate a stack to hold the last button that was clicked.
        }

        /*
         * Given the alphabet, generates and returns 7 random letters which will be used as the 
         * letters to choose from. Will ensure that the resulting char[] will contain minimum two vowels (A, E, I, O, U)
         */
        private char[] GenerateRandomLetters(String alphabet)
        {
            // Create new Random
            Random rand = new Random();
            char[] letters = new char[7];

            // While there are not two vowels
            while (!ContainsMinTwoVowels(letters))
            {
                // We need to add letters at random
                int i = 0;
                while (i < letters.Length)
                {
                    letters[i] = alphabet[rand.Next(26)];
                    i++;
                }

            }

            return letters;
        }

        /*
         * Determines if a given list contains at minimum two vowels.
         */
        private Boolean ContainsMinTwoVowels(char[] list)
        {
            if (list == null || list.Length == 0)
            {
                return false;
            }
            int vowels = 0;
            foreach (char item in list)
            {
                if (item == 'A' || item == 'E' || item == 'I' || item == 'O' || item == 'U')
                {
                    vowels++;
                }
            }
            return vowels >= 2; // Returns true if we have the number of vowels we need.
        }

        /*
         * Adds the "controls" to the window (Done, Backspace, and Fill buttons). 
         */
        private void AddControls()
        {
            controlPanel = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.Center };
            done = new Button { Text = "Done", BackgroundColor = Color.BurlyWood, BorderWidth = 1, BorderColor = Color.Black };
            done.Clicked += DoneClicked;
            backspace = new Button { Text = "Backspace", BackgroundColor = Color.BurlyWood, BorderWidth = 1, BorderColor = Color.Black };
            backspace.Clicked += BackSpaceClicked;
            fill = new Button { Text = "Fill", BackgroundColor = Color.BurlyWood, BorderWidth = 1, BorderColor = Color.Black };
            fill.Clicked += FillRandom;
            newGame = new Button { Text = "New Game", BackgroundColor = Color.BurlyWood, BorderWidth = 1, BorderColor = Color.Black };
            newGame.Clicked += NewGameRequest;

            controlPanel.Children.Add(done);
            controlPanel.Children.Add(backspace);
            controlPanel.Children.Add(fill);
            controlPanel.Children.Add(newGame);
        }

        /*
         * Creates a new game with new tiles. Essentially resets the window
         */
        private void NewGameRequest(object sender, EventArgs e)
        {
            // Clear the stacklayout
            buttonContainer.Children.Clear();
            AddButtons();
            GetAvailableWords();
            doneWords.Clear();
            wordBuilder.Text = "";
            numFormed = 0;
            numWords.Text = numFormed + "/" + totWords; // Reset for new word count
            done.IsEnabled = true;
            backspace.IsEnabled = true;
            fill.IsEnabled = true;
        }

        /*
         * Adds the listview to the Window, as well as creates the ObservableCollection for real-time updates.
         */
        private void AddListView()
        {
            completedWords = new StackLayout { Orientation = StackOrientation.Horizontal };
            doneWords = new ObservableCollection<string>();
            lps = new ListView();
            completedWords.Children.Add(lps);
            lps.ItemsSource = doneWords;

        }

        /*
         * Creates 50 arbitrary strings and adds them to the ListView
         * to demonstrate scrolling
         */
        private void FillRandom(Object sender, EventArgs e)
        {
            for (int i = 0; i < 50; i++)
            {
                doneWords.Add("Random String  #" + i);
            }
        }

        /*
         * Finds and re-enabled the last button clicked
         */
        private void FindEnableButton()
        {
            if (lastClickedButtons.Count != 0)
            {
                foreach (ButtonID item in buttonArray)
                {
                    if (item.IDNum == lastClickedButtons.Peek())
                    {
                        item.IsEnabled = true;
                    }
                }
                lastClickedButtons.Pop();
            }

        }

        //================================================================================================================= Derived Classes
        /*
         * Class derived from button which extends the functionality to have a specific ID.
         * This will aid in re-enabling clicked buttons when the user performs a backspace.
         */
        public class ButtonID : Button
        {
            public int IDNum { get; set; }
        }


    }
}
