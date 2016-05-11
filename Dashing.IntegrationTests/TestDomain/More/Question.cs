namespace Dashing.IntegrationTests.TestDomain.More {
    public class Question {
        public long QuestionId { get; set; }

        public Questionnaire Questionnaire { get; set; }

        public string Name { get; set; }
    }
}