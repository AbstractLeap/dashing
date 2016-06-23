namespace Dashing.IntegrationTests.TestDomain.More {
    using System.Collections.Generic;

    public class Questionnaire {
        public long QuestionnaireId { get; set; }

        public IList<Question> Questions { get; set; }

        public string Name { get; set; }
    }
}