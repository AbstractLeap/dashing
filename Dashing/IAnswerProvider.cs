namespace Dashing {
    using System.Collections.Generic;

    public interface IAnswerProvider {
        bool GetBooleanAnswer(string question);

        MultipleChoice<T> GetMultipleChoiceAnswer<T>(string question, IEnumerable<MultipleChoice<T>> choices);

        T GetAnswer<T>(string question) where T : struct;
    }
}