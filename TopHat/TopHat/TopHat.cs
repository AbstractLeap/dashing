using TopHat.Configuration;

namespace TopHat {
	public static class TopHat {
		public static DefaultConfiguration Configure(IEngine engine, string connectionString) {
			return new DefaultConfiguration(engine, connectionString);
		}
	}
}