namespace Dashing.Console {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Tools;

    internal class ConsoleAnswerProvider : IAnswerProvider {
        public bool GetBooleanAnswer(string question) {
            Console.WriteLine(question + " (y/n)");
            var answer = Console.ReadLine().ToLowerInvariant().Trim();
            if (answer == "yes" || answer == "y" || answer == "true") {
                return true;
            }

            return false;
        }

        public MultipleChoice<T> GetMultipleChoiceAnswer<T>(string question, IEnumerable<MultipleChoice<T>> choices) {
            var multipleChoices = choices as MultipleChoice<T>[] ?? choices.ToArray();

            // ask the question
            Console.WriteLine();
            using (Color(ConsoleColor.Green)) {
                Console.WriteLine(question);
            }
            
            // lay out the answers
            var i = 1;
            foreach (var option in multipleChoices) {
                Console.WriteLine(i++ + ") " + option.DisplayString);
            }

            // prompt
            var readLine = string.Empty;
            int number;
            var prompt = "Enter " + string.Join(", ", Enumerable.Range(1, multipleChoices.Count() - 1)) + " or " + multipleChoices.Count() + ": ";
            Console.WriteLine(prompt);

            // first attempt
            readLine = Console.ReadLine().Trim();

            // now prompt again until they answer
            while (!int.TryParse(readLine, out number)) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine(prompt);
                }

                readLine = Console.ReadLine().Trim();
            }

            Console.WriteLine();
            return multipleChoices.ElementAt(number);
        }

        private static ColorContext Color(ConsoleColor color) {
            return new ColorContext(color);
        }
    }
}