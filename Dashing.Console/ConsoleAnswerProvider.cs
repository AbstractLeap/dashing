namespace Dashing.Console {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Dashing.Tools;

    internal class ConsoleAnswerProvider : IAnswerProvider {
        private readonly string persistenceFilePath;

        public ConsoleAnswerProvider(string persistenceFilePath = null) {
            this.persistenceFilePath = persistenceFilePath;
        }

        private bool TryGetAnswer(string question, out string answer) {
            answer = this.LoadPersistedAnswers().FirstOrDefault(pair => pair.Key.Equals(question)).Value;
            return !string.IsNullOrEmpty(answer);
        }

        private IEnumerable<KeyValuePair<string, string>> LoadPersistedAnswers() {
            if (this.persistenceFilePath == null || !File.Exists(this.persistenceFilePath)) {
                yield break;
            }

            using (var stream = File.OpenRead(this.persistenceFilePath)) {
                using (var reader = new BinaryReader(stream, Encoding.UTF8)) {
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; ++i) {
                        var key = reader.ReadString();
                        var value = reader.ReadString();
                        yield return new KeyValuePair<string, string>(key, value);
                    }
                }
            }
        }

        private void SavePersistedAnswer(string question, string answer) {
            if (this.persistenceFilePath == null) {
                return;
            }

            var answers = this.LoadPersistedAnswers().GroupBy(p => p.Key).ToDictionary(g => g.Key, g => g.Last().Value);
            answers[question] = answer;

            using (var stream = File.Open(this.persistenceFilePath, FileMode.Create, FileAccess.Write)) {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8)) {
                    writer.Write(answers.Count());
                    foreach (var pair in answers) {
                        writer.Write(pair.Key);
                        writer.Write(pair.Value);
                    }
                }
            }
        }

        public bool GetBooleanAnswer(string question) {
            string answer;
            if (!this.TryGetAnswer(question, out answer)) {
                Console.WriteLine(question + " (y/n)");
                answer = Console.ReadLine().ToLowerInvariant().Trim();
                this.SavePersistedAnswer(question, answer);
            }

            if (answer == "yes" || answer == "y" || answer == "true") {
                return true;
            }

            return false;
        }

        public MultipleChoice<T> GetMultipleChoiceAnswer<T>(string question, IEnumerable<MultipleChoice<T>> choices) {
            int number;
            var multipleChoices = choices as MultipleChoice<T>[] ?? choices.ToArray();

            string answer;
            if (this.TryGetAnswer(question, out answer)) {
                if (int.TryParse(answer, out number)) {
                    if (0 <= number && number < multipleChoices.Length) {
                        var multipleChoiceAnswer = multipleChoices.ElementAt(number);

                        // print a little reminder
                        using (Color(ConsoleColor.DarkGreen)) {
                            Console.WriteLine("-- " + question);
                            Console.WriteLine("-- Remembered answer: " + multipleChoiceAnswer.DisplayString);
                        }

                        return multipleChoiceAnswer;
                    }
                }
            }

            // ask the question
            Console.WriteLine();
            using (Color(ConsoleColor.Green)) {
                Console.WriteLine(question);
            }

            // lay out the answers
            var i = 0;
            foreach (var option in multipleChoices) {
                Console.WriteLine(i++ + ") " + option.DisplayString);
            }

            // prompt
            var prompt = "Enter " + string.Join(", ", Enumerable.Range(0, multipleChoices.Count() - 1)) + " or " + (multipleChoices.Count() - 1)
                         + ": ";
            Console.WriteLine(prompt);

            // first attempt
            answer = Console.ReadLine().Trim();

            // now prompt again until they answer
            while (!int.TryParse(answer, out number)) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine(prompt);
                }

                answer = Console.ReadLine().Trim();
            }

            this.SavePersistedAnswer(question, answer);

            Console.WriteLine();
            return multipleChoices.ElementAt(number);
        }

        public T GetAnswer<T>(string question) where T : struct {
            string answer;
            if (this.TryGetAnswer(question, out answer)) {
                try {
                    var typedAnswer = (T)Convert.ChangeType(answer, typeof(T));
                    return typedAnswer;
                }
                catch {
                    // doesn't matter here, we'll fall back to asking the user
                }
            }

            // ask the user
            Console.WriteLine();
            using (Color(ConsoleColor.Green)) {
                Console.WriteLine(question);
            }

            while (true) {
                answer = Console.ReadLine();
                try {
                    var theTypedAnswer = (T)Convert.ChangeType(answer, typeof(T));
                    this.SavePersistedAnswer(question, answer);
                    return theTypedAnswer;
                }
                catch (InvalidCastException ex) {
                    Console.WriteLine("There was an invalid cast exception: " + ex.Message + ". Please re-try:");
                }
                catch (FormatException ex) {
                    Console.WriteLine("There was a format exception: " + ex.Message + ". Please re-try:");
                } // let the others go
            }
        }

        private static ColorContext Color(ConsoleColor color) {
            return new ColorContext(color);
        }
    }
}