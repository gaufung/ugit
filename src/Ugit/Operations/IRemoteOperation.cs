namespace Ugit
{
    internal interface IRemoteOperation
    {
        void Fetch();

        void Push(string refName);
    }
}
