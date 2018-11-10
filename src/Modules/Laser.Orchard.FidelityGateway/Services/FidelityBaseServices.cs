using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using Laser.Orchard.FidelityGateway.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;
using Orchard.Data;
using System.Linq;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using Orchard.Localization;
using Laser.Orchard.FidelityGateway.Activities;
using Orchard.Workflows.Services;
using System.Net.Mail;
using Orchard.Users.Models;

namespace Laser.Orchard.FidelityGateway.Services
{
    public abstract class FidelityBaseServices : IFidelityServices
    {
        protected readonly IOrchardServices _orchardServices;
        protected readonly IEncryptionService _encryptionService;
        protected readonly IAuthenticationService _authenticationService;
        protected readonly IMembershipService _membershipService;
        protected readonly ISendService _sendService;
        protected readonly FidelitySettingsPart settingsPart;
        protected readonly IRepository<ActionInCampaignRecord> _actionInCampaign;
        protected readonly IWorkflowManager _workflowManager;

        public FidelityBaseServices(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService,
                               ISendService sendService, IRepository<ActionInCampaignRecord> repository, IWorkflowManager workfloManager)
        {
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _sendService = sendService;
            _actionInCampaign = repository;
            settingsPart = _orchardServices.WorkContext.CurrentSite.As<FidelitySettingsPart>();
            _workflowManager = workfloManager;
        }

        //ritorna il nome del provider attivo, utilizzato anche per la corrispondenza con il provider della tabella azionicampagne, (nome tutto minuscolo)
        public abstract string GetProviderName();

        public virtual APIResult<IEnumerable<ActionInCampaignRecord>> GetActions()
        {
            APIResult<IEnumerable<ActionInCampaignRecord>> api = new APIResult<IEnumerable<ActionInCampaignRecord>>();
            api.data = _actionInCampaign.Fetch(a => a.Provider.Equals(GetProviderName()));
            api.success = true;
            api.message = "ritorno delle azioni impostate su Krake per " + GetProviderName() + ".";
            return api;
        }


        public virtual APIResult<CardPointsCampaign> AddPointsFromAction(string action, string customerId)
        {
            int id;
            Int32.TryParse(action, out id);
            ActionInCampaignRecord actionrecord = _actionInCampaign.Get(a => a.Id == id && a.Provider.Equals(GetProviderName()));
            if (actionrecord == null)
            {
                return new APIResult<CardPointsCampaign>() { success = false, data = null, message = "azione inesistente" };
            }
            else
            {
                return AddPoints(actionrecord.Points.ToString(), actionrecord.CampaignId, customerId);
            }
        }

        public APIResult<FidelityCustomer> CreateFidelityAccountFromCookie(string campaignId)
        {
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null)
            {

                FidelityUserPart fideliyPart = (FidelityUserPart)(((dynamic)authenticatedUser.ContentItem).FidelityUserPart);

                if (fideliyPart != null)
                {
                    return CreateFidelityAccount(fideliyPart, authenticatedUser.UserName, authenticatedUser.Email, campaignId);
                }
                else
                    return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is already register in " + GetProviderName() };
            }
            else
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "Cookie not provided or not valid." };
        }

        public virtual APIResult<FidelityCustomer> CreateFidelityAccountFromCookie()
        {
            if (!string.IsNullOrWhiteSpace(settingsPart.DefaultCampaign))
            {
                return CreateFidelityAccountFromCookie(settingsPart.DefaultCampaign);
            }
            else
            {
                return new APIResult<FidelityCustomer>() { data = null, message = "no default campaign setted", success = false };
            }

        }

        public virtual APIResult<FidelityCustomer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email, string campaignId)
        {
            if (fidelityPart != null && !String.IsNullOrWhiteSpace(username))
            {
                FidelityCustomer customer = new FidelityCustomer(email, username, Membership.GeneratePassword(12, 4));

                APIResult<FidelityCustomer> creationRequest = _sendService.SendCustomerRegistration(settingsPart, customer, campaignId);

                if (creationRequest.success)
                {
                    fidelityPart.FidelityUsername = customer.Username;
                    fidelityPart.FidelityPassword = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(customer.Password)));
                    if (!string.IsNullOrWhiteSpace(customer.Id))
                    {
                        fidelityPart.CustomerId = customer.Id;
                    }
                }
                return creationRequest;
            }
            else
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
        }

        public virtual APIResult<FidelityCustomer> GetCustomerDetails(string customerId)
        {
            FidelityCustomer customer;
            if (customerId == null)
            {
                customer = GetCustomerFromAuthenticatedUser();
            }
            else
            {
                customer = GetCustomerFromIdOrEmail(customerId);
            }

            if (customer != null)
            {
                return _sendService.SendCustomerDetails(settingsPart, customer); ;
            }
            else
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }
        }

        public virtual APIResult<FidelityCampaign> GetCampaignData(string id)
        {
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.Id = id;
            return _sendService.SendCampaignData(settingsPart, campaign);
        }

        public virtual APIResult<CardPointsCampaign> AddPoints(string numPoints, string campaignId, string customerId)
        {
            APIResult<CardPointsCampaign> res = new APIResult<CardPointsCampaign>();
            FidelityCustomer customer;
            if (customerId == null)
            {
                customer = GetCustomerFromAuthenticatedUser();
            }
            else
            {
                customer = GetCustomerFromIdOrEmail(customerId);
            }

            FidelityCampaign campaign = new FidelityCampaign();
            campaign.Id = campaignId;
            if (customer != null)
            {
                APIResult<bool> resAdd = _sendService.SendAddPoints(settingsPart, customer, campaign, numPoints);
                if (!resAdd.success)
                {
                    res = new APIResult<CardPointsCampaign> { success = false, data = null, message = resAdd.message };
                }
                else
                {
                    APIResult<FidelityCustomer> resCust = _sendService.SendCustomerDetails(settingsPart, customer);
                    if (!resCust.success)
                    {
                        res = new APIResult<CardPointsCampaign> { success = false, data = null, message = resCust.message };
                    }
                    else
                    {
                        res.success = resAdd.success;
                        res.message = resAdd.message;
                        CardPointsCampaign data = new CardPointsCampaign()
                        {
                            idCampaign = campaignId,
                            idCustomer = customer.Id,
                            points = resCust.data.PointsInCampaign[campaignId]
                        };
                        res.data = data;
                    }
                }
            }
            else
            {
                res = new APIResult<CardPointsCampaign> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }
            _workflowManager.TriggerEvent("AddFidelityPoints", null, () => new Dictionary<string, object> {
                {"result", res}
            });
            return res;
        }

        public virtual APIResult<FidelityReward> GiveReward(string rewardId, string campaignId, string customerId)
        {
            APIResult<FidelityReward> res = new APIResult<FidelityReward>();
            FidelityCustomer customer;
            if (customerId == null)
            {
                customer = GetCustomerFromAuthenticatedUser();
            }
            else 
            {
                customer = GetCustomerFromIdOrEmail(customerId);
            }
          
            if (customer != null)
            {
                FidelityReward reward = new FidelityReward();
                reward.Id = rewardId;
                FidelityCampaign campaign = new FidelityCampaign();
                campaign.Id = campaignId;
                APIResult<bool> resGive =  _sendService.SendGiveReward(settingsPart, customer, reward, campaign);
                if (!resGive.success)
                {
                    res = new APIResult<FidelityReward> { success = false, data = null, message = resGive.message };
                }
                else
                {
                    APIResult<FidelityCampaign> resCamp = _sendService.SendCampaignData(settingsPart, campaign);
                    if (!resCamp.success)
                    {
                        res = new APIResult<FidelityReward> { success = false, data = null, message = resCamp.message };
                    }
                    else
                    {
                        res.message = resGive.message;
                        res.success = resGive.success;
                        res.data = resCamp.data.Rewards.Where(r => r.Id.Equals(rewardId)).First();
                    }             
                }                
            }
            else
            {
               res =  new APIResult<FidelityReward> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }
            _workflowManager.TriggerEvent("RedeemFidelityReward", null, () => new Dictionary<string, object> {
                {"result", res}
            });
            return res;
        }

        public virtual APIResult<IEnumerable<FidelityCampaign>> GetCampaignList()
        {
            return _sendService.SendCampaignList(settingsPart);
        }


        //DEFAULT CAMPAIGN
        public APIResult<FidelityCampaign> GetCampaignData()
        {
            if (!string.IsNullOrWhiteSpace(settingsPart.DefaultCampaign))
            {
                return GetCampaignData(settingsPart.DefaultCampaign);
            }
            else
            {
                return new APIResult<FidelityCampaign>() { data = null, message = "no default campaign setted", success = false };
            }
        }

        public APIResult<FidelityReward> GiveReward(string rewardId, string customerId)
        {
            if (!string.IsNullOrWhiteSpace(settingsPart.DefaultCampaign))
            {
                return GiveReward(rewardId, settingsPart.DefaultCampaign, customerId);
            }
            else
            {
                return new APIResult<FidelityReward>() { data = null, message = "no default campaign setted", success = false };
            }
        }

        public APIResult<CardPointsCampaign> AddPoints(string amount, string customerId)
        {
            if (!string.IsNullOrWhiteSpace(settingsPart.DefaultCampaign))
            {
                return AddPoints(amount, settingsPart.DefaultCampaign, customerId);
            }
            else
            {
                return new APIResult<CardPointsCampaign>() { data = null, message = "no default campaign setted", success = false };
            }
        }

        /// <summary>
        /// Ritorna l'il FidelityCustomer associato all'User autenticato su Orchard
        /// </summary>
        /// <returns>FidelityCustomer se esiste un utente autenticato, null altrimenti</returns>
        public virtual FidelityCustomer GetCustomerFromAuthenticatedUser()
        {
           
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null)
            {
                FidelityUserPart fidelityPart = (FidelityUserPart)(((dynamic)authenticatedUser.ContentItem).FidelityUserPart);

                if (fidelityPart != null && !String.IsNullOrWhiteSpace(fidelityPart.FidelityUsername)
                    && !String.IsNullOrWhiteSpace(fidelityPart.FidelityPassword)
                    )
                {
                    string pass = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(fidelityPart.FidelityPassword)));
                    FidelityCustomer customer = new FidelityCustomer(authenticatedUser.Email, fidelityPart.FidelityUsername, pass);
                    if (String.IsNullOrWhiteSpace(fidelityPart.CustomerId)) {
                        fidelityPart.CustomerId = _sendService.SendCustomerDetails(settingsPart, customer).data.Id;
                    }
                    customer.Id = fidelityPart.CustomerId;
                    return customer;
                }
            }
            return null;
        }

        public virtual FidelityCustomer GetCustomerFromIdOrEmail(string custId)
        {
            FidelityUserPart fidelityPart = null;
            if (IsValidEmail(custId))
            {
                var userPart = _orchardServices.ContentManager.HqlQuery().ForPart<UserPart>().Where(a => a.ContentPartRecord<UserPartRecord>(), x => x.Eq("Email", custId)).List().FirstOrDefault();
                if (userPart == null)
                {
                    return null;
                }
                fidelityPart = userPart.ContentItem.As<FidelityUserPart>();                    
            }
            else
            {
                var userList = _orchardServices.ContentManager.Query<FidelityUserPart, FidelityUserPartRecord>().Where(x => x.CustomerId == custId).List().ToList();
                try
                {
                    fidelityPart = userList.First();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
          
                string pass = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(fidelityPart.FidelityPassword)));
                return new FidelityCustomer { Id = fidelityPart.CustomerId, Username = fidelityPart.FidelityUsername, Password = pass };
        }

        public bool IsValidEmail(string email){
            try
            {
                MailAddress m = new MailAddress(email);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}