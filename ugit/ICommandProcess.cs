using System;

namespace ugit
{
    public interface ICommandProcess
    {
        ValueTuple<int, string, string> Execute(string command, string arguments);
    }
}