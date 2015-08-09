namespace Dashing.IntegrationTests.TestDomain.More.MultipleFetchManyWithNonRootAndThenFetchDomain {
    using System.Collections.Generic;

    public class QuestionnaireResponse {
        public virtual int QuestionnaireResponseId { get; set; }

        public virtual Questionnaire Questionnaire { get; set; }

        public virtual Booking Booking { get; set; }

        public virtual IList<QuestionResponse> Responses { get; set; }
    }

    public class QuestionResponse {
        public virtual int QuestionResponseId { get; set; }

        public virtual QuestionnaireResponse QuestionnaireResponse { get; set; }

        public virtual Question Question { get; set; }
    }

    public class Question {
        public virtual int QuestionId { get; set; }

        public virtual Questionnaire Questionnaire { get; set; }

        public virtual string Name { get; set; }
    }

    public class Booking {
        public virtual int BookingId { get; set; }

        public virtual bool IsFoo { get; set; }

        public virtual IList<Bed> Beds { get; set; }
    }

    public class Bed {
        public virtual int BedId { get; set; }

        public virtual RoomSlot RoomSlot { get; set; }

        public virtual Booking Booking { get; set; }
    }

    public class RoomSlot {
        public virtual int RoomSlotId { get; set; }

        public virtual Room Room { get; set; }
    }

    public class Room {
        public virtual int RoomId { get; set; }

        public virtual string Name { get; set; }
    }

    public class Questionnaire {
        public virtual int QuestionnaireId { get; set; }

        public virtual IList<Question> Questions { get; set; }

        public virtual string Name { get; set; }
    }
}