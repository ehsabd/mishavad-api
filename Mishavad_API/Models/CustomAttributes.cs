using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mishavad_API.Models
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class Encrypted : System.Attribute
    {
        public Encrypted() { }
    }


    public enum ContextGeneratedOption
    {
        None=0,
        Random=1
    }
    /// <summary>
    /// Binary file position to locate Key and IV for cryptography
    /// The annotated property type should be typeof(long)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BF_Idx : System.Attribute //Binary file index, used to store and retrieve IV and Key for cryptography
    {
        public ContextGeneratedOption ContextGenerated { get; set; }
        public BF_Idx(ContextGeneratedOption option=ContextGeneratedOption.Random) {
            ContextGenerated = option;
        }
    }

   

}