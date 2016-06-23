namespace Dashing.IntegrationTests.TestDomain.More {
    public class QuestionResponse {
        public long QuestionResponseId { get; set; }

        public QuestionnaireResponse QuestionnaireResponse { get; set; }

        public Question Question { get; set; }
    }
}