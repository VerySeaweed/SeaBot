using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBot
{
    internal class DataBase
    {
        internal List<object> Data { get; } = new List<object>();
        
        internal void AddData(object data)
        {
            Data.Add(data);
        }

        internal void RemoveData(object data)
        {
            int index = Data.IndexOf(data);
            Data.RemoveAt(index);
        }

        internal void RemoveData(int index)
        {
            Data.RemoveAt(index);
        }
    }
}
