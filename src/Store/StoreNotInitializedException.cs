using System;

namespace AReSSO.Store
{
    public class StoreNotInitializedException : Exception
    {
        public StoreNotInitializedException()
            : base(
                "This Store has not been initialized. To initialize a Store you must dispatch the " +
                "InitializeAction to it."
            )
        {
        }
    }
}