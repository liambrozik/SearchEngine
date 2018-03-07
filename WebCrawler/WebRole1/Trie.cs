using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebRole1
{
    public class Trie
    {
        private readonly Node _root;

        public Trie()
        {
            _root = new Node('*', 0, null);
        }

        public Node Prefix(string s)
        {
            var rootNode = this._root;
            var currentNode = rootNode;

            foreach (char c in s)
            {
                if (currentNode.Children.Count == 0)
                {
                    break;
                }
                foreach (Node child in currentNode.Children)
                {
                    if (Char.ToLower(child.Value) == Char.ToLower(c))
                    {
                        currentNode = child;
                        break;
                    }
                }
            }
            return currentNode;
            /*
            var currentNode = _root;
            var result = currentNode;

            foreach (var c in s)
            {
                currentNode = currentNode.FindChild(c);
                //if (currentNode == null)
                if (currentNode.Children.Count == 0)
                    break;
                result = currentNode;
            }

            return result;*/
        }

        /*
        public List<string> PrefixSearch(string s)
        {
            List<string> results = new List<string>();
            Node currentNode = Prefix(s);
            if (currentNode.Depth == s.Length && currentNode.Value != '*')
            {
                Console.WriteLine(currentNode.Value);
                foreach (Node child in currentNode.Children)
                {
                    GetSuggestions(child, s, results);
                }
            }
            else
            {
                results.Add("");
            }
            return results;
        }*/

        public void GetSuggestions(Node n, string s, List<string> l)
        {
            if (l.Count < 11)
            {
                if (n.Suffix.Contains("@"))
                {
                    l.Add(s);
                }
                if (n.Children.Count == 0)
                {
                    int index = 0;
                    int offset = 0;
                    if (n.Suffix.Contains("@"))
                    {
                        offset = 1;
                    }
                    while (l.Count < 11 && index < n.Suffix.Count - offset)
                    {
                        if (n.Suffix[index] != "@")
                        {
                            l.Add(s + n.Suffix[index]);
                        }
                        index++;
                    }
                }
                else
                {
                    foreach (Node child in n.Children)
                    {
                        if (n.Value == '*')
                        {
                            GetSuggestions(child, s, l);
                        }
                        else
                        {
                            GetSuggestions(child, s + child.Value, l);
                        }
                    }
                }
            }
            /*
            char current = n.Value;
            if (l.Count < 10)
            {


                if (current == '@')
                {
                    l.Add(s);
                }
                else
                {

                    foreach (Node child in n.Children)
                    {
                        GetSuggestions(child, s + current, l);
                    }
                }
            }*/

        }
        //public void Insert(string s)
        public void Insert(string s, Node n)
        {
            var commonPrefix = n;
            var currentNode = commonPrefix;
            if (s == "" || s == null || s.Length == 0)
            {
                currentNode.Suffix.Add("@");
            }
            else
            {

                if (currentNode.Children.Count == 0)
                {
                    if (currentNode.Suffix.Count < 50)
                    {
                        currentNode.Suffix.Add(s);
                    }
                    else
                    {
                        var newNode = new Node(s[0], currentNode.Depth + 1, currentNode);
                        currentNode.Children.Add(newNode);
                        Insert(s.Substring(1), newNode);
                        for (int i = 0; i < currentNode.Suffix.Count; i++)
                        {
                            Insert(currentNode.Suffix[i], n);
                        }
                        string add = "";
                        if (currentNode.Suffix.Contains("@"))
                        {
                            add = "@";
                        }
                        currentNode.Suffix.Clear();
                        currentNode.Suffix.Add(add);

                    }
                }
                else
                {
                    Node child = n.FindChild(s[0]);
                    if (child == n)
                    {
                        var newNode = new Node(s[0], currentNode.Depth + 1, currentNode);
                        currentNode.Children.Add(newNode);
                        Insert(s.Substring(1), newNode);
                    }
                    else
                    {
                        Insert(s.Substring(1), child);
                    }
                }
            }
            /*
            var commonPrefix = Prefix(s);
            var currentNode = commonPrefix;
            for (var i = currentNode.Depth; i < s.Length; i++)
            {
                var newNode = new Node(s[i], currentNode.Depth + 1, currentNode);
                currentNode.Children.Add(newNode);
                currentNode = newNode;
            }

            currentNode.Children.Add(new Node('@', currentNode.Depth + 1, currentNode));
            */
        }

        public Node GetRoot()
        {
            return this._root;
        }

    }
}
