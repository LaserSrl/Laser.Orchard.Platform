using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityReward
    {
        public string Id { set; get; }
        public string Description { set; get; }
        public string Name { set; get; }

        /// <summary>
        /// Dati aggiuntivi
        /// </summary>
        public Dictionary<string, string> Data { set; get; }

        public FidelityReward()
        {
            Data = new Dictionary<string, string>();
        }

        public bool AddData(string k, string v)
        {
            try
            {
                Data.Add(k, v);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        } 
    }
}