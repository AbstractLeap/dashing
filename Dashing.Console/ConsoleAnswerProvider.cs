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
            Console.WriteLine(question);
            var i = 1;
            foreach (var option in choices) {
                Console.WriteLine(i + " : " + option.DisplayString);
            }

            Console.WriteLine("Please enter the number for the correct option?");
            var result = Console.ReadLine().Trim();
            int number;
            if (!int.TryParse(result, out number)) {
                Console.WriteLine("Please enter a number");
                return GetMultipleChoiceAnswer(question, choices);
            }

            return choices.ElementAt(number);
        }
    }
}