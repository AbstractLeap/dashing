namespace TopHat.Tracking {
  public class ChangeTrackingBehaviourFactory : IBehaviourFactory {
    public IBehaviour GetBehaviour() {
      return new ChangeTrackingBehaviour();
    }

    public string Name {
      get {
        return "ChangeTracking";
      }
    }
  }
}