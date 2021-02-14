using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoitaMod.Util
{
    public class Singleton<T> where T : class, new()
    {
        protected static T instance;
        public static T Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new T();
                }

                return instance;
            }
        }
    }
}
