namespace Dashing.Tools {
    using System.Collections.Generic;

    public interface IAnswerProvider {
        bool GetBooleanAnswer(string question);

        MultipleChoice<T> GetMultipleChoiceAnswer<T>(string question, IEnumerable<MultipleChoice<T>> choices);
    }
}