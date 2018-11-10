using System;
using System.Collections.Generic;
using Laser.Orchard.FidelityGateway.Services;
using Laser.Orchard.FidelityGateway.Models;
using System.Net.Http;
using System.Net;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Laser.Orchard.FidelitySimsol.Services
{
    public class SimsolSendService : ISendService
    {

        public APIResult<bool> SendAddPoints(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityCampaign campaign, string points)
        {
            APIResult<bool> result = new APIResult<bool>();
            result.success = false;
            result.data = false;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("campaign_id", campaign.Id),
                    new KeyValuePair<string, string>("code", customer.Id),
                    new KeyValuePair<string, string>("amount", points),
                };
                string responseString = SendRequest(setPart, "record_activity", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<string>("status").Equals("success");
                    if (result.success)
                    {
                        result.data = true;
                        result.message = "Simsol points added with success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("errors").ToString();
                    }
                }
                else
                {
                    result.message = "no response from Simsol server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "exception: " + ex.Message + ".";
            }
            return result;
        }

        public APIResult<FidelityCampaign> SendCampaignData(FidelitySettingsPart setPart, FidelityCampaign campaign)
        {
            APIResult<FidelityCampaign> result = new APIResult<FidelityCampaign>();
            result.data = null;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("campaign_id", campaign.Id)
                };
                string responseString = SendRequest(setPart, "campaign_rewards", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<string>("status").Equals("success");
                    if (result.success)
                    {
                        AddRewardsInCampaignFromToken(data.SelectToken("rewards"), campaign);
                        result.data = campaign;
                        result.message = "Simsol campaign data request success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("errors").ToString();
                    }
                }
                else
                {
                    result.success = false;
                    result.message = "no response from Simsol server.";
                }
            }
            catch (Exception ex)
            {
                result.success = false;
                result.message = "exception: " + ex.Message + ";";
            }
            return result;
        }

        public APIResult<IEnumerable<FidelityCampaign>> SendCampaignList(FidelitySettingsPart setPart)
        {
            APIResult<IEnumerable<FidelityCampaign>> result = new APIResult<IEnumerable<FidelityCampaign>>();
            List<FidelityCampaign> listCamp = new List<FidelityCampaign>();
            result.data = null;
            result.success = false;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>();
                string responseString = SendRequest(setPart, "campaigns_list", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<string>("status").Equals("success");
                    if (result.success)
                    {
                        result.message = "campaign list";
                        result.data = CreateCampaignListFromToken(data.SelectToken("campaigns"));
                    }
                    else
                    {
                        result.message = data.SelectToken("errors").ToString();
                    }
                }
                else
                {
                    result.message = "no response from Simsol server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "Exception: " + ex.Message + " in Simsol Campaign List.";
            }
            return result;
        }

        public APIResult<FidelityCustomer> SendCustomerDetails(FidelitySettingsPart setPart, FidelityCustomer customer)
        {
            string responseString = string.Empty;
            APIResult<FidelityCustomer> result = new APIResult<FidelityCustomer>();
            result.data = null;
            result.success = false;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("code",customer.Id),
                };
                responseString = SendRequest(setPart, "customer_info", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<string>("status").Equals("success");
                    if (result.success)
                    {
                        customer.Data = this.DictionaryFromResponseToken(data.SelectToken("customer"));
                        RemoveCustomerPropertyFromDataDictionary(customer);
                        AddPointsCampaignToCustomer(data.SelectToken("campaigns"), customer);
                        result.data = customer;
                        result.message = "Simsol customer login success.";
                    }
                    else
                    {
                        result.message = data.SelectToken("errors").ToString();
                    }
                }
                else
                {
                    result.message = "no response from Simsol server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "Exception: " + ex.Message + " in Simsol Login.";
            }
            return result;
        }

        public APIResult<FidelityCustomer> SendCustomerRegistration(FidelitySettingsPart setPart, FidelityCustomer customer, string campaignId)
        {

            APIResult<FidelityCustomer> result = new APIResult<FidelityCustomer>();
            result.data = null;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("customer_username", customer.Username ),
                    new KeyValuePair<string, string>("customer_password", customer.Password),
                    new KeyValuePair<string, string>("first_name", customer.Username),
                    new KeyValuePair<string, string>("email",customer.Email), 
                    new KeyValuePair<string, string>("campaign_id", campaignId),
                    new KeyValuePair<string, string>("customer_action", "new"),
                    new KeyValuePair<string, string>("card_number_generate", "10")
                };
                string responseString = SendRequest(setPart, "record_customer", kvpList);

                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<string>("status").Equals("success");
                    if (result.success)
                    {
                        customer.Id = data.SelectToken("customer").Value<string>("code");
                        result.data = customer;
                        result.message = "registrazione effettuata con successo";
                    }
                    else
                    {
                        result.message = data.SelectToken("errors").ToString();
                    }
                }
                else
                {
                    result.success = false;
                    result.message = "no response from Simsol server.";
                }
            }
            catch (Exception ex)
            {
                result.success = false;
                result.message = "exception: " + ex.Message + ".";
            }
            return result;
        }

        public APIResult<string> SendGetMerchantId(FidelitySettingsPart setPart)
        {
            throw new NotImplementedException();
        }

        public APIResult<bool> SendGiveReward(FidelitySettingsPart setPart, FidelityCustomer customer, FidelityReward reward, FidelityCampaign campaign)
        {
            APIResult<bool> result = new APIResult<bool>();
            result.data = false;
            try
            {
                List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("campaign_id", campaign.Id),
                    new KeyValuePair<string, string>("code", customer.Id),
                    new KeyValuePair<string, string>("reward_to_redeem", reward.Id),
                };
                string responseString = SendRequest(setPart, "redeem", kvpList);
                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    JObject data = JObject.Parse(responseString);
                    result.success = data.Value<string>("status").Equals("success");
                    if (result.success)
                    {
                        result.data = true;
                        result.message = "Simsol reward gived with success.";
                    }
                    else
                    {
                        result.message = "The reward level selected exceeds the points available";
                    }
                }
                else
                {
                    result.message = "no response from Loyalzoo server.";
                }
            }
            catch (Exception ex)
            {
                result.message = "exception: " + ex.Message + ".";
            }
            return result;
        }

        /// <summary>
        /// Invia la richiesta all'URL API
        /// </summary>
        /// <param name="APIType">Tipologia di API da richiamare <see cref="APIType"/></param>
        /// <param name="APIMetodo">Metodo da richiamare</param>
        /// <param name="kvpList">Elenco dei parametri da passare al provider come parametri;</param>
        /// <returns>Restituisce una stringa in formato json</returns>
        private static string SendRequest(FidelitySettingsPart setPart, string APIMetodo, List<KeyValuePair<string, string>> kvpList)
        {
            string responseString = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    Uri address = new Uri(setPart.ApiURL);
                    kvpList.Add(new KeyValuePair<string, string>("user_id", setPart.UserID));
                    kvpList.Add(new KeyValuePair<string, string>("user_password", setPart.DeveloperKey));
                    kvpList.Add(new KeyValuePair<string, string>("account_id", setPart.AccountID));
                    kvpList.Add(new KeyValuePair<string, string>("type", APIMetodo));
                    kvpList.Add(new KeyValuePair<string, string>("output", "JSON"));
                    var content = new FormUrlEncodedContent(kvpList);
                    HttpResponseMessage result = client.PostAsync(address, content).Result;
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        responseString = result.Content.ReadAsStringAsync().Result;
                        if (responseString.Contains("\"status\":errors")) { throw new Exception(responseString); } //TODO vedere se ha senso mantenere il lancio dell'eccezione qui e tutti i controlli sui metodi dopo
                    }
                    else if (result.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("Direttiva non valida");
                    }
                    else
                    {
                        throw new Exception(result.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return responseString;
        }

        //crea un dizionario strig string partendo da un JToken
        private Dictionary<string, string> DictionaryFromResponseToken(JToken token)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            JObject response = JObject.Parse(token.ToString());
            foreach (KeyValuePair<string, JToken> entry in response)
            {
                data.Add(entry.Key, entry.Value.ToString());
            }
            return data;
        }

        //elimina dal dizionario data tutti i campi che sono gia presenti nelle propietà dal customer
        //inoltre aggiunge la lista dei punti acquisiti
        private void RemoveCustomerPropertyFromDataDictionary(FidelityCustomer customer)
        {
            Dictionary<string, string> data = customer.Data;
            if (data.ContainsKey("email"))
            {
                data.Remove("email");
            }
            if (data.ContainsKey("code"))
            {
                data.Remove("id");
            }
            if (data.ContainsKey("customer_username"))
            {
                data.Remove("customer_username");
            }
            //if (data.ContainsKey("rewards"))
            //{
            //    AddPointsInPlaceToCustomer(data["rewards"], customer);
            //    data.Remove("rewards");
            //}
        }

        //partendo dal JToken crea la lista delle campagne
        private List<FidelityCampaign> CreateCampaignListFromToken(JToken tokenRewards)
        {
            List<FidelityCampaign> list = new List<FidelityCampaign>();
            if (tokenRewards.Children().Count() > 0)
            {
                foreach (JToken tokenCamp in tokenRewards.Children())
                {
                    FidelityCampaign campaign = new FidelityCampaign();
                    campaign.Id = tokenCamp.Value<string>("id");
                    campaign.Name = tokenCamp.Value<string>("name");
                    try
                    {
                        campaign.AddData("description", tokenCamp.SelectToken("description").ToString());
                    }
                    catch
                    {
                        campaign.AddData("description", "no description");
                    }
                    campaign.AddData("type", tokenCamp.SelectToken("type").ToString());

                    //addReward
                    list.Add(campaign);
                }
            }
            return list;
        }

        //partendo dal JToken che rappresenta il catalogo setta la propietà catalogo dalla campaign
        private void AddRewardsInCampaignFromToken(JToken tokenRewards, FidelityCampaign campaign)
        {
            if (tokenRewards.Children().Count() > 0)
            {
                foreach (JToken tokenRew in tokenRewards.Children())
                {
                    FidelityReward reward = new FidelityReward();
                    reward.Id = tokenRew.Value<string>("id");
                    reward.Description = tokenRew.Value<string>("description");
                    campaign.Rewards.Add(reward);
                    campaign.AddReward(reward.Id, tokenRew.Value<string>("level"));
                }
            }
        }

        private void AddPointsCampaignToCustomer(JToken tokenCampaigns, FidelityCustomer customer)
        {
            if (tokenCampaigns.Children().Count() > 0)
            {
                foreach (JToken tokenCamp in tokenCampaigns.Children())
                {
                    customer.SetPointsCampaign(tokenCamp.Value<string>("id"), tokenCamp.Value<double>("balance"));
                }
            }
        }

        public APIResult<IDictionary<string, string>> GetOtherSettings(FidelitySettingsPart setPart)
        {
            throw new NotImplementedException();
        }
    }
}