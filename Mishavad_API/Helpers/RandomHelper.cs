using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mishavad_API.Helpers
{
    public class RandomHelper
    {
        /*NOTE: this variable will be initialized sometime! We do not know when but we know it will at most once in 
        our application*/
        public static Random SimpleRandom = new Random();
    }
}