// Tools.cs
namespace FunctionCalling.Tools
{
    using System.Collections.Generic;

    public class Tool
    {
        public string Type { get; set; }
        public FunctionDetail Function { get; set; }
    }

    public class FunctionDetail
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Parameters Parameters { get; set; }
    }

    public class Parameters
    {
        public string Type { get; set; }
        public Properties Properties { get; set; }
        public List<string> Required { get; set; }
    }

    public class Properties
    {
        public Location Location { get; set; }
        public Unit Unit { get; set; }
    }

    public class Location
    {
        public string Type { get; set; }
        public string Description { get; set; }
    }

    public class Unit
    {
        public string Type { get; set; }
        public List<string> Enum { get; set; }
    }
}
