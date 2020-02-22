namespace AReSSO
{
    public class Store
    {
        public StateNode State { get; private set; }

        public Store(StateNode initialState) => State = initialState;
    }
}