using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimenter
{
    class ParseJson
    {


        public double Distance { get; set; }

        public ParseJson(string Json)
        {
            Distance = getDistance(Json);
        }



        #region parsing functions
       

        private double getDistance(string json)
        {
            string search1 = "\"travelDistance\":";
            int found1 = json.LastIndexOf(search1);
            int found2 = json.LastIndexOf(",\"travelDuration\":");

            string temp = json.Substring(found1, found2 - found1);
            temp = temp.Remove(0, search1.Length);
            temp = temp.Replace(".", ",");

            double distance = Convert.ToDouble(temp);

            return distance;
            

        }

        #endregion 
    }

 
}
