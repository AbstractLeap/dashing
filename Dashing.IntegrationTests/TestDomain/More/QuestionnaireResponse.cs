namespace Dashing.IntegrationTests.TestDomain.More {
    using System.Collections.Generic;

    public class QuestionnaireResponse {
        public long QuestionnaireResponseId { get; set; }

        public Questionnaire Questionnaire { get; set; }

        public Booking Booking { get; set; }

        public IList<QuestionResponse> Responses { get; set; }
    }
}