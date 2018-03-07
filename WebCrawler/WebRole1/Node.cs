using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebRole1
{
    public class Node
    {
        public char Value { get; set; }
        public List<Node> Children { get; set; }
        public List<string> Suffix { get; set; }
        public int Depth { get; set; }

        public Node(char value, int depth, Node parent)
        {
            Value = value;
            Children = new List<Node>();
            Suffix = new List<string>();
            Depth = depth;
        }
        public Node FindChild(char c)
        {
            if (Children.Count == 0)
            {
                return this;
            }
            else
            {
                foreach (var child in Children)
                    if (Char.ToLower(child.Value) == Char.ToLower(c))
                        return child;

                return this;

            }
            /*
            foreach (var child in Children)
                if (Char.ToLower(child.Value) == Char.ToLower(c))
                    return child;

            return null;*/
        }
    }
}
