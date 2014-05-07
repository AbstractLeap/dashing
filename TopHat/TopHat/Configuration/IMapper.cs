using System;

namespace TopHat.Configuration {
	public interface IMapper {
		Map MapFor(Type entity);

		Map<T> MapFor<T>();
	}
}