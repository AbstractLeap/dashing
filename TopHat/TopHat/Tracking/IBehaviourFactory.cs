namespace TopHat.Tracking {
  public interface IBehaviourFactory {
    IBehaviour GetBehaviour();

    string Name { get; }
  }
}