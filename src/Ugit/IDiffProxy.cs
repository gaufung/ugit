namespace Ugit
{
    internal interface IDiffProxy
    {
        (int, string, string) Execute(string name, string arguments);
    }
}
