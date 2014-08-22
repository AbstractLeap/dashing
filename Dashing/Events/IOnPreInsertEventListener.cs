using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Events {
    public interface IOnPreInsertEventListener : IEventListener {
        void OnPreInsert(object entity, ISession session);
    }
}
