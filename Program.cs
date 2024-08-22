using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace FinalProjectC_
{
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public List<QuizResult> Results { get; set; } = new List<QuizResult>();
    }

    public class Quiz
    {
        public string Title { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<QuizResult> Results { get; set; } = new List<QuizResult>();
    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public List<int> CorrectAnswers { get; set; } = new List<int>();
    }

    public class QuizResult
    {
        public User User { get; set; }
        public Quiz Quiz { get; set; }
        public int CorrectAnswersCount { get; set; }
        public DateTime DateTaken { get; set; }
    }

    public class AuthService
    {
        private List<User> users = new List<User>();

        public bool Register(string username, string password, DateTime dateOfBirth)
        {
            if (users.Any(u => u.Login == username))
                return false; 

            var user = new User
            {
                Login = username,
                Password = HashPassword(password),
                DateOfBirth = dateOfBirth
            };
            users.Add(user);
            return true;
        }



        public User Login(string username, string password)
        {
            var user = users.FirstOrDefault(u => u.Login == username && u.Password == HashPassword(password));
            return user;
        }

        public void ChangePassword(User user, string newPassword)
        {
            user.Password = HashPassword(newPassword);
        }

        public void ChangeDateOfBirth(User user, DateTime newDateOfBirth)
        {
            user.DateOfBirth = newDateOfBirth;
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    public class QuizApp
    {
        private AuthService authService = new AuthService();
        private List<Quiz> quizzes = new List<Quiz>();
        private User currentUser;

        public void Start()
        {
            Console.WriteLine("Hello!");
            InitializeQuizzes();
            ShowLoginMenu();
        }

        private void ShowLoginMenu()
        {
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("0. Exit");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    Register();
                    break;
                case "0":
                    Exit();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    ShowLoginMenu();
                    break;
            }
        }

        private void Exit()
        {
            Console.WriteLine("Exiting the application. Goodbye!");
            Environment.Exit(0);
        }

        public class LoginRepository
        {
            private static LoginRepository _instance;
            private Dictionary<string, string> _loginsAndPasswords;

            private LoginRepository()
            {
                _loginsAndPasswords = new Dictionary<string, string>();
                ReadLoginsAndPasswordsFromFile();
            }

            public static LoginRepository Instance
            {
                get { return _instance ?? (_instance = new LoginRepository()); }
            }

            public bool IsValidLogin(string login, string password)
            {
                return _loginsAndPasswords.ContainsKey(login) && _loginsAndPasswords[login] == password;
            }

            private void ReadLoginsAndPasswordsFromFile()
            {
                string filePath = ConfigurationManager.AppSettings["LoginsAndPasswordsFilePath"];
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new Exception("File path not configured.");
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("The specified file was not found.", filePath);
                }

                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        _loginsAndPasswords.Add(parts[0], parts[1]);
                    }
                    else
                    {
                        throw new FormatException("The file format is incorrect.");
                    }
                }
            }
        }

        private void Login()
        {
            string username;
            while (true)
            {
                Console.Write("Login: ");
                username = Console.ReadLine();
                if (username.Length >= 5)
                    break;
                else
                    Console.WriteLine("Login must be at least 5 characters long. Please try again.");
            }

            Console.Write("Password: ");
            string password;
            while (true)
            {
                password = Console.ReadLine();
                if (password.Length >= 8)
                    break;
                else
                    Console.WriteLine("Password must be at least 8 characters long. Please try again.");
            }

            var user = authService.Login(username, password);
            if (user != null)
            {
                currentUser = user;
                ShowMainMenu();
            }
            else
            {
                Console.WriteLine("Incorrect login or password. Try again.");
                ShowLoginMenu();
            }
        }


        private void Register()
        {
            string login;
            while (true)
            {
                Console.Write("Login (min 5 characters): ");
                login = Console.ReadLine();
                if (login.Length >= 5)
                    break;
                else
                    Console.WriteLine("Login must be at least 5 characters long. Please try again.");
            }

            string password;
            while (true)
            {
                Console.Write("Password (min 8 characters): ");
                password = Console.ReadLine();
                if (password.Length >= 8)
                    break;
                else
                    Console.WriteLine("Password must be at least 8 characters long. Please try again.");
            }

            DateTime dob;
            while (true)
            {
                Console.Write("Date of Birth (yyyy-mm-dd): ");
                try
                {
                    dob = DateTime.Parse(Console.ReadLine());
                    if (dob <= DateTime.Today)
                        break; 
                    else
                        Console.WriteLine("Date of Birth cannot be in the future. Please try again.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid date format. Please enter the date in the format yyyy-mm-dd.");
                }
            }

            if (authService.Register(login, password, dob))
            {
                Console.WriteLine("Registration successful.");
                ShowLoginMenu();
            }
            else
            {
                Console.WriteLine("Login already exists.");
                Register();
            }
        }


        private void ShowMainMenu()
        {
            Console.WriteLine("1. Start New Quiz");
            Console.WriteLine("2. View Past Results");
            Console.WriteLine("3. View Top-20");
            Console.WriteLine("4. Settings");
            Console.WriteLine("5. Edit quiz");
            Console.WriteLine("6. Create new quiz");
            Console.WriteLine("7. Logout");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    StartNewQuiz();
                    break;
                case "2":
                    ViewPastResults();
                    break;
                case "3":
                    ViewTop20();
                    break;
                case "4":
                    ChangeSettings();
                    break;
                case "5":
                    EditQuiz();
                    break;
                case "6":
                    CreateNewQuiz();
                    break;
                case "7":
                    Logout();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please select a number between 1 and 5.");
                    ShowMainMenu(); 
                    break;
            }
        }

        private string adminLogin = "admin";
        private string adminPassword = "01082008";

        private void EditQuiz()
        {
            Console.Write("Enter admin login: ");
            string login = Console.ReadLine();
            Console.Write("Enter admin password: ");
            string password = Console.ReadLine();

            if (login == adminLogin && password == adminPassword)
            {
                Console.WriteLine("Select a quiz to edit:");
                for (int i = 0; i < quizzes.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {quizzes[i].Title}");
                }

                int quizIndex;
                while (true)
                {
                    if (int.TryParse(Console.ReadLine(), out quizIndex) && quizIndex >= 1 && quizIndex <= quizzes.Count)
                    {
                        break;
                    }
                    Console.WriteLine("Invalid choice. Please enter a number between 1 and " + quizzes.Count + ":");
                }

                quizIndex -= 1; 
                var quiz = quizzes[quizIndex];

                while (true)
                {
                    Console.WriteLine("Edit quiz options:");
                    Console.WriteLine("1. Add a new question");
                    Console.WriteLine("2. Edit an existing question");
                    Console.WriteLine("3. Delete a question");
                    Console.WriteLine("4. Save changes and exit");

                    int choice = int.Parse(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            AddNewQuestion(quiz);
                            break;
                        case 2:
                            EditExistingQuestion(quiz);
                            break;
                        case 3:
                            DeleteQuestion(quiz);
                            break;
                        case 4:
                            Console.WriteLine("Changes saved successfully!");
                            ShowMainMenu();
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Try again.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid admin credentials.");
                ShowMainMenu();
            }
        }

        private void CreateNewQuiz()
        {
            Console.Write("Enter admin login: ");
            string login = Console.ReadLine();
            Console.Write("Enter admin password: ");
            string password = Console.ReadLine();

            if (login == adminLogin && password == adminPassword)
            {
                Console.WriteLine("Create a new quiz:");
                Console.Write("Enter quiz title: ");
                string title = Console.ReadLine();

                var quiz = new Quiz { Title = title, Questions = new List<Question>() };

                while (true)
                {
                    Console.WriteLine("Add a new question:");
                    Console.Write("Enter question text: ");
                    string questionText = Console.ReadLine();

                    var question = new Question { Text = questionText, Options = new List<string>(), CorrectAnswers = new List<int>() };

                    Console.Write("Enter options (separated by commas): ");
                    string options = Console.ReadLine();
                    question.Options = options.Split(',').Select(o => o.Trim()).ToList();

                    Console.Write("Enter correct answer(s) (separated by commas): ");
                    string correctAnswers = Console.ReadLine();
                    question.CorrectAnswers = correctAnswers.Split(',').Select(int.Parse).ToList();

                    quiz.Questions.Add(question);

                    Console.WriteLine("Add another question? (y/n)");
                    string response = Console.ReadLine();
                    if (response.ToLower() != "y")
                        break;
                }

                quizzes.Add(quiz);
                Console.WriteLine("Quiz created successfully!");

                ShowMainMenu();
            }
            else
            {
                Console.WriteLine("Invalid admin credentials.");
                ShowMainMenu();
            }
        }

        private void AddNewQuestion(Quiz quiz)
        {
            Console.WriteLine("Add a new question:");
            Console.Write("Enter question text: ");
            string questionText = Console.ReadLine();

            var question = new Question { Text = questionText, Options = new List<string>(), CorrectAnswers = new List<int>() };

            Console.Write("Enter options (separated by commas): ");
            string options = Console.ReadLine();
            question.Options = options.Split(',').Select(o => o.Trim()).ToList();

            Console.Write("Enter correct answer(s) (separated by commas): ");
            string correctAnswers = Console.ReadLine();
            question.CorrectAnswers = correctAnswers.Split(',').Select(int.Parse).ToList();

            quiz.Questions.Add(question);
            Console.WriteLine("Question added successfully!");
        }

        private void EditExistingQuestion(Quiz quiz)
        {
            Console.WriteLine("Select a question to edit:");
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {quiz.Questions[i].Text}");
            }

            int questionIndex = int.Parse(Console.ReadLine()) - 1;
            var question = quiz.Questions[questionIndex];

            Console.WriteLine("Edit question options:");
            Console.WriteLine("1. Edit question text");
            Console.WriteLine("2. Edit options");
            Console.WriteLine("3. Edit correct answers");
            Console.WriteLine("4. Back to quiz menu");

            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.Write("Enter new question text: ");
                    question.Text = Console.ReadLine();
                    break;
                case 2:
                    Console.Write("Enter new options (separated by commas): ");
                    string options = Console.ReadLine();
                    question.Options = options.Split(',').Select(o => o.Trim()).ToList();
                    break;
                case 3:
                    Console.Write("Enter new correct answer(s) (separated by commas): ");
                    string correctAnswers = Console.ReadLine();
                    question.CorrectAnswers = correctAnswers.Split(',').Select(int.Parse).ToList();
                    break;
                case 4:
                    return;
                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            }
        }

        private void DeleteQuestion(Quiz quiz)
        {
            Console.WriteLine("Select a question to delete:");
            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {quiz.Questions[i].Text}");
            }

            int questionIndex = int.Parse(Console.ReadLine()) - 1;
            quiz.Questions.RemoveAt(questionIndex);
            Console.WriteLine("Question deleted successfully!");
        }

        private static void ShuffleQuestions(List<Question> questions)
        {
            Random rng = new Random();
            int n = questions.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Question value = questions[k];
                questions[k] = questions[n];
                questions[n] = value;
            }
        }
        private void StartNewQuiz()
        {
            Console.WriteLine("Choose a quiz:");
            for (int i = 0; i < quizzes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {quizzes[i].Title}");
            }
            Console.WriteLine($"{quizzes.Count + 1}. Mixed Quiz (random questions from all quizzes)");

            int quizIndex;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out quizIndex) && quizIndex >= 1 && quizIndex <= quizzes.Count + 1)
                {
                    break;
                }
                Console.WriteLine("Invalid choice. Please enter a number between 1 and " + (quizzes.Count + 1) + ":");
            }

            Quiz selectedQuiz;
            if (quizIndex == quizzes.Count + 1)
            {
                selectedQuiz = new Quiz { Title = "Mixed Quiz" };
                var allQuestions = quizzes.SelectMany(q => q.Questions).ToList();
                ShuffleQuestions(allQuestions);
                selectedQuiz.Questions = allQuestions.Take(20).ToList();
            }
            else
            {
                quizIndex -= 1; 
                selectedQuiz = quizzes[quizIndex];
            }

            int correctAnswers = 0;
            foreach (var question in selectedQuiz.Questions)
            {
                bool validInput = false;
                List<int> userAnswer = null;

                while (!validInput)
                {
                    Console.WriteLine(question.Text);
                    for (int i = 0; i < question.Options.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {question.Options[i]}");
                    }

                    try
                    {
                        var userInput = Console.ReadLine();
                        userAnswer = userInput.Split(',').Select(s => int.Parse(s)).ToList();

                        if (userAnswer.Any(a => a < 1 || a > question.Options.Count))
                        {
                            throw new Exception("Input out of range.");
                        }

                        validInput = true;
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input. Please enter valid option numbers separated by commas.");
                    }
                }

                if (userAnswer.OrderBy(x => x).SequenceEqual(question.CorrectAnswers.OrderBy(x => x)))
                {
                    correctAnswers++;
                }
            }

            var result = new QuizResult
            {
                User = currentUser,
                Quiz = selectedQuiz,
                CorrectAnswersCount = correctAnswers,
                DateTaken = DateTime.Now
            };
            selectedQuiz.Results.Add(result);
            currentUser.Results.Add(result);

            Console.WriteLine($"You got {correctAnswers} out of {selectedQuiz.Questions.Count} correct!");

            var rankings = selectedQuiz.Results.OrderByDescending(r => r.CorrectAnswersCount).ThenBy(r => r.DateTaken).ToList();
            var userRank = rankings.IndexOf(result) + 1;

            Console.WriteLine($"You ranked #{userRank} out of {rankings.Count} players!");
            ShowMainMenu();
        }


        private List<int> GetValidAnswers(int numberOfOptions)
        {
            List<int> answers;
            while (true)
            {
                Console.Write("Enter your answers (comma separated): ");
                var input = Console.ReadLine();
                var parts = input.Split(',').Select(s => s.Trim()).ToList();

                if (parts.All(p => int.TryParse(p, out int num) && num >= 1 && num <= numberOfOptions))
                {
                    answers = parts.Select(int.Parse).ToList();
                    break;
                }
                else
                {
                    Console.WriteLine($"Invalid input. Please enter numbers between 1 and {numberOfOptions}, separated by commas.");
                }
            }
            return answers;
        }

        private int GetValidInteger(int minValue, int maxValue)
        {
            int result;
            while (true)
            {
                Console.Write("Enter your choice: ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out result) && result >= minValue && result <= maxValue)
                {
                    break;
                }
                else
                {
                    Console.WriteLine($"Invalid input. Please enter a number between {minValue} and {maxValue}.");
                }
            }
            return result;
        }

        public class Result
        {
            public DateTime DateTaken { get; set; }
            public User User { get; set; }
            public int CorrectAnswersCount { get; set; }
            public Quiz Quiz { get; set; }

            public Result(DateTime dateTaken, User user, int correctAnswersCount, Quiz quiz)
            {
                DateTaken = dateTaken;
                User = user;
                CorrectAnswersCount = correctAnswersCount;
                Quiz = quiz;
            }
        }

        private static List<Result> results = new List<Result>();
        private void ViewPastResults()
        {
            var latestResults = results.OrderByDescending(r => r.DateTaken).Take(10).ToList();

            if (latestResults.Count == 0)
            {
                Console.WriteLine("No results to display.");
                ShowMainMenu();
                return;
            }

            Console.WriteLine("Latest Results:");
            for (int i = 0; i < latestResults.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {latestResults[i].User.Login}: {latestResults[i].CorrectAnswersCount}/{latestResults[i].Quiz.Questions.Count} correct on {latestResults[i].DateTaken}");
            }

            ShowMainMenu();
        }

        private void ViewTop20()
        {
            Console.WriteLine("Choose Quiz to View Top-20:");
            for (int i = 0; i < quizzes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {quizzes[i].Title}");
            }
            int quizChoice = int.Parse(Console.ReadLine()) - 1;

            if (quizChoice < 0 || quizChoice >= quizzes.Count)
            {
                Console.WriteLine("Invalid choice.");
                ViewTop20();
                return;
            }

            var selectedQuiz = quizzes[quizChoice];
            var topResults = selectedQuiz.Results.OrderByDescending(r => r.CorrectAnswersCount)
                                                 .ThenBy(r => r.DateTaken)
                                                 .Take(20)
                                                 .ToList();

            if (topResults.Count == 0)
            {
                Console.WriteLine("No results to display.");
                ShowMainMenu();
                return;
            }

            Console.WriteLine($"Top-20 for {selectedQuiz.Title}:");
            for (int i = 0; i < topResults.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {topResults[i].User.Login}: {topResults[i].CorrectAnswersCount}/{selectedQuiz.Questions.Count} correct on {topResults[i].DateTaken}");
            }

            ShowMainMenu();
        }

        private void ChangeSettings()
        {
            while (true)
            {
                Console.WriteLine("1. Change Password");
                Console.WriteLine("2. Change Date of Birth");
                Console.Write("Enter your choice: ");
                var choice = Console.ReadLine();

                if (choice == "1")
                {
                    string newPassword;
                    while (true)
                    {
                        Console.Write("Enter new password (min 8 characters): ");
                        newPassword = Console.ReadLine();
                        if (newPassword.Length >= 8)
                            break;
                        else
                            Console.WriteLine("The password must contain at least 8 characters. Try again.");
                    }

                    authService.ChangePassword(currentUser, newPassword);
                    Console.WriteLine("Password changed successfully.");
                    break; 
                }
                else if (choice == "2")
                {
                    DateTime newDob;
                    while (true)
                    {
                        Console.Write("Enter new Date of Birth (yyyy-mm-dd): ");
                        try
                        {
                            newDob = DateTime.Parse(Console.ReadLine());
                            if (newDob <= DateTime.Today)
                                break; 
                            else
                                Console.WriteLine("Date of birth cannot be in the future. Please try again.");
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Date of birth entered incorrectly. Try again.");
                        }
                    }

                    authService.ChangeDateOfBirth(currentUser, newDob);
                    Console.WriteLine("Date of Birth changed successfully.");
                    break; 
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please enter 1 or 2.");
                }
            }

            ShowMainMenu();
        }

        private void Logout()
        {
            currentUser = null;
            ShowLoginMenu();
        }

        private void InitializeQuizzes()
        {
            var geographyQuiz = new Quiz
            {
                Title = "Geography",
                Questions = new List<Question>
        {
            new Question { Text = "What is the capital of France?", Options = new List<string> { "Berlin", "Madrid", "Paris", "Rome" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which is the longest river in the world?", Options = new List<string> { "Amazon", "Nile", "Yangtze", "Mississippi" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which continent is the Sahara Desert located on?", Options = new List<string> { "Asia", "Africa", "Australia", "South America" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "What is the smallest country in the world?", Options = new List<string> { "Monaco", "Vatican City", "San Marino", "Liechtenstein" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which ocean is the largest?", Options = new List<string> { "Atlantic Ocean", "Indian Ocean", "Arctic Ocean", "Pacific Ocean" }, CorrectAnswers = new List<int> { 4 } },
            new Question { Text = "What is the tallest mountain in the world?", Options = new List<string> { "K2", "Mount Everest", "Kangchenjunga", "Lhotse" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which country has the most population?", Options = new List<string> { "India", "USA", "China", "Indonesia" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "What is the largest continent?", Options = new List<string> { "Africa", "Asia", "Europe", "South America" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which river runs through Egypt?", Options = new List<string> { "Amazon", "Yangtze", "Nile", "Ganges" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which city is known as the 'City of Love'?", Options = new List<string> { "London", "Paris", "Venice", "New York" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which country is known as the Land of the Rising Sun?", Options = new List<string> { "China", "Japan", "Thailand", "South Korea" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "What is the currency of the United Kingdom?", Options = new List<string> { "Euro", "Pound Sterling", "Dollar", "Yen" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which US state is known as the Sunshine State?", Options = new List<string> { "California", "Florida", "Texas", "Hawaii" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "What is the official language of Brazil?", Options = new List<string> { "Spanish", "Portuguese", "French", "English" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which country is the Eiffel Tower located in?", Options = new List<string> { "Spain", "Italy", "France", "Germany" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which country is famous for the Great Wall?", Options = new List<string> { "Japan", "China", "Russia", "India" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which city is known as the Big Apple?", Options = new List<string> { "Los Angeles", "Chicago", "Miami", "New York" }, CorrectAnswers = new List<int> { 4 } },
            new Question { Text = "Which of the following cities are located on the Danube River?", Options = new List<string> { "Vienna", "Budapest", "Belgrade", "Prague" }, CorrectAnswers = new List<int> { 1, 2, 3 } },
            new Question { Text = "Which of the following countries are landlocked?", Options = new List<string> { "Austria", "Switzerland", "Hungary", "Czech Republic" }, CorrectAnswers = new List<int> { 1, 2, 3, 4 } },
            new Question { Text = "Which of the following mountains are part of the Alps?", Options = new List<string> { "Mont Blanc", "Matterhorn", "Eiger", "Pyrenees" }, CorrectAnswers = new List<int> { 1, 2, 3 } },
        }
            };

            var historyQuiz = new Quiz
            {
                Title = "History",
                Questions = new List<Question>
        {
            new Question { Text = "Who was the first President of the United States?", Options = new List<string> { "Abraham Lincoln", "George Washington", "Thomas Jefferson", "John Adams" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "In which year did World War II end?", Options = new List<string> { "1941", "1943", "1945", "1947" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which ancient civilization built the pyramids?", Options = new List<string> { "Roman", "Greek", "Egyptian", "Mayan" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Who discovered America?", Options = new List<string> { "Christopher Columbus", "Ferdinand Magellan", "Marco Polo", "Leif Erikson" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "In which year did the Titanic sink?", Options = new List<string> { "1910", "1911", "1912", "1913" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Who was the last Emperor of Russia?", Options = new List<string> { "Nicholas I", "Alexander III", "Nicholas II", "Peter the Great" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which war was fought between the North and South regions in the United States?", Options = new List<string> { "World War I", "World War II", "The Civil War", "The Revolutionary War" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Who was the British Prime Minister during World War II?", Options = new List<string> { "Winston Churchill", "Neville Chamberlain", "Clement Attlee", "Anthony Eden" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "Which empire was known as the 'Empire on which the sun never sets'?", Options = new List<string> { "Roman Empire", "Mongol Empire", "British Empire", "Ottoman Empire" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which event began on July 28, 1914?", Options = new List<string> { "World War I", "World War II", "The Great Depression", "The American Civil War" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "Who was the first man to set foot on the moon?", Options = new List<string> { "Buzz Aldrin", "Yuri Gagarin", "Neil Armstrong", "Michael Collins" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which country was the main opponent of the United States during the Cold War?", Options = new List<string> { "China", "Germany", "Soviet Union", "Cuba" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "In which year did the Berlin Wall fall?", Options = new List<string> { "1987", "1988", "1989", "1990" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which famous leader was assassinated on April 4, 1968?", Options = new List<string> { "John F. Kennedy", "Mahatma Gandhi", "Martin Luther King Jr.", "Malcolm X" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Who was the first female Prime Minister of the United Kingdom?", Options = new List<string> { "Theresa May", "Margaret Thatcher", "Angela Merkel", "Indira Gandhi" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "What was the main cause of World War I?", Options = new List<string> { "Assassination of Archduke Franz Ferdinand", "Treaty of Versailles", "Great Depression", "Hitler's invasion of Poland" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "Which battle is considered the turning point of the American             Civil War?", Options = new List<string> { "Battle of Gettysburg", "Battle of Antietam", "Battle of Fort Sumter", "Battle of Bull Run" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "Who were the leaders of the Soviet Union during World War II?", Options = new List<string> { "Joseph Stalin", "Vladimir Lenin", "Leon Trotsky", "Georgy Zhukov" }, CorrectAnswers = new List<int> { 1, 4 } },
            new Question { Text = "Which of the following ancient civilizations built the Great Library of Alexandria?", Options = new List<string> { "Egyptians", "Greeks", "Romans", "Babylonians" }, CorrectAnswers = new List<int> { 1, 2 } },
            new Question { Text = "Which of the following events occurred during the American Revolution?", Options = new List<string> { "Boston Tea Party", "Declaration of Independence", "Battle of Yorktown", "Treaty of Paris" }, CorrectAnswers = new List<int> { 1, 2, 3, 4 } },
        }
            };

            var biologyQuiz = new Quiz
            {
                Title = "Biology",
                Questions = new List<Question>
        {
            new Question { Text = "What is the basic unit of life?", Options = new List<string> { "Atom", "Molecule", "Cell", "Organ" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which organelle is known as the powerhouse of the cell?", Options = new List<string> { "Nucleus", "Ribosome", "Mitochondria", "Chloroplast" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which molecule carries genetic information?", Options = new List<string> { "DNA", "RNA", "Protein", "Lipid" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "Which process do plants use to make their food?", Options = new List<string> { "Respiration", "Photosynthesis", "Digestion", "Fermentation" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which type of blood cells help fight infections?", Options = new List<string> { "Red blood cells", "White blood cells", "Platelets", "Plasma cells" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "What is the largest organ in the human body?", Options = new List<string> { "Heart", "Liver", "Skin", "Lungs" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which vitamin is produced in the skin when exposed to sunlight?", Options = new List<string> { "Vitamin A", "Vitamin B", "Vitamin C", "Vitamin D" }, CorrectAnswers = new List<int> { 4 } },
            new Question { Text = "Which part of the plant conducts photosynthesis?", Options = new List<string> { "Root", "Stem", "Leaf", "Flower" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "What is the chemical formula for water?", Options = new List<string> { "H2O", "CO2", "O2", "H2" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "Which organ filters blood in the human body?", Options = new List<string> { "Heart", "Kidney", "Liver", "Lungs" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which part of the cell contains the genetic material?", Options = new List<string> { "Cytoplasm", "Nucleus", "Cell membrane", "Mitochondria" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which blood type is known as the universal donor?", Options = new List<string> { "A", "B", "AB", "O" }, CorrectAnswers = new List<int> { 4 } },
            new Question { Text = "Which process involves cell division for growth and repair?", Options = new List<string> { "Mitosis", "Meiosis", "Fission", "Fusion" }, CorrectAnswers = new List<int> { 1 } },
            new Question { Text = "What type of organism is a yeast?", Options = new List<string> { "Bacteria", "Fungi", "Plant", "Animal" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "Which animal is known for changing its color?", Options = new List<string> { "Octopus", "Chameleon", "Jellyfish", "Salamander" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "What is the main gas that plants absorb during photosynthesis?", Options = new List<string> { "Oxygen", "Nitrogen", "Carbon dioxide", "Hydrogen" }, CorrectAnswers = new List<int> { 3 } },
            new Question { Text = "Which structure in the human body connects muscles to bones?", Options = new List<string> { "Ligament", "Tendon", "Cartilage", "Nerve" }, CorrectAnswers = new List<int> { 2 } },
            new Question { Text = "What are the processes by which plants produce energy?", Options = new List<string> { "Respiration", "Photosynthesis", "Transpiration", "Fermentation" }, CorrectAnswers = new List<int> { 2, 3 } },
            new Question { Text = "Which of the following types of rocks are formed from the cooling and solidification of magma?", Options = new List<string> { "Igneous", "Sedimentary", "Metamorphic", "Foliated" }, CorrectAnswers = new List<int> { 1, 3 } },
            new Question { Text = "Which of the following scientists are credited with the discovery of DNA structure?", Options = new List<string> { "James Watson", "Francis Crick", "Rosalind Franklin", "Maurice Wilkins" }, CorrectAnswers = new List<int> { 1, 2, 3, 4 } },
        }
            };

            quizzes.Add(geographyQuiz);
            quizzes.Add(historyQuiz);
            quizzes.Add(biologyQuiz);
        }

        static void Main(string[] args)
        {
            var app = new QuizApp();
            app.Start();
        }
    }
}

