using AJCCFM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AJCCFM
{
    public class FolderComparer : IEqualityComparer<Folder>
    {
        public bool Equals(Folder x, Folder y)
        {
            //Check whether the objects are the same object. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether the products' properties are equal. 


            return x != null && y != null && x.FolderName.Equals(y.FolderName);
        }

            public int GetHashCode(Folder obj)
        {
            int hashProductName = obj.FolderName == null ? 0 : obj.FolderName.GetHashCode();
            return hashProductName;

        }
    }
}