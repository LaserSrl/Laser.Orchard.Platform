using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityCampaign
    {
        public string Name { set; get; }
        public string Id { set; get; }
        public Dictionary<string, double> Catalog {set; get;}
        public List<FidelityReward> Rewards { set; get; }
        /// <summary>
        /// Dati aggiuntivi
        /// </summary>
        public Dictionary<string, string> Data { set; get; }

        public FidelityCampaign()
        {
            Catalog = new Dictionary<string, double>();
            Data = new Dictionary<string, string>();
            Rewards = new List<FidelityReward>();
        }

        public bool AddReward(string rewardId, string points)
        {
            try
            {
                double dpoints;
                if (double.TryParse(points, out dpoints))
                {
                    Catalog.Add(rewardId, dpoints);
                }
                else
                {
                    return false;
                }               
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
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