using DzuoolIoT.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzuoolIoT.DataModels
{
    public class Acceler : BaseIoT, IJson
    {
        [JsonProperty("AcX")]
        public double AcceraX { get; set; }
        [JsonProperty("AcY")]
        public double AcceraY { get; set; }
        [JsonProperty("AcZ")]
        public double AcceraZ { get; set; }

        public string ToJson()
        {
            throw new NotImplementedException();
        }

        public static Acceler FromJson(string data)
        {
            Acceler obj = JsonConvert.DeserializeObject<Acceler>(data);
            if (null == obj) return null;

            return obj;
        }
    }
}
