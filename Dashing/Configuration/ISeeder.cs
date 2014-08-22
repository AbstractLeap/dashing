namespace Dashing.Configuration {
    public interface ISeeder {
        void Seed(ISession session);
    }
}