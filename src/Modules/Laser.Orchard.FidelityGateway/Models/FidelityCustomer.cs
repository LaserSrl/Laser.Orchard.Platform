using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityCustomer
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Id { get; set; }
        public Dictionary<string, double> PointsInCampaign { get; set; }

        /// <summary>
        /// Dati aggiuntivi
        /// </summary>
        public Dictionary<string, string> Data { get; set; }

        public FidelityCustomer()
        {
            PointsInCampaign = new Dictionary<string, double>();
            Data = new Dictionary<string, string>();
        }

        public FidelityCustomer(string email, string username, string password) : this()
        {
            this.Email = email;
            this.Username = username;
            this.Password = password;
        }

        public void SetPointsCampaign(string campaign_id, double points)
        {
            PointsInCampaign[campaign_id] = points;
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