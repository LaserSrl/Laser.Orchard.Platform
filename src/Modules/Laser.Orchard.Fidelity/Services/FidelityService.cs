using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.ViewModels;
using Loyalzoo;
using Loyalzoo.DomainObject;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Laser.Orchard.Fidelity.Services
{
    public class FidelityService : IFidelityService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;

        public FidelityService(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService)
        {
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
        }

        public APIResult CreateLoyalzooAccountFromCookie()
        {
            try
            {
                var authenticatedUser = _authenticationService.GetAuthenticatedUser();
                if (authenticatedUser != null)
                {
                    LoyalzooUserPart loyalzooPart = authenticatedUser.As<LoyalzooUserPart>();

                    if (loyalzooPart != null)
                    {
                        return CreateLoyalzooAccount(loyalzooPart, authenticatedUser.UserName, authenticatedUser.Email);
                    }
                    else
                        return new APIResult { success = false, data = null, message = "The user is not configured to use Loyalzoo." };
                }
                else
                    return new APIResult { success = false, data = null, message = "Cookie not provided or not valid." };

            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult CreateLoyalzooAccountFromContext(CreateContentContext context)
        {
            try
            {
                IUser userPart = context.ContentItem.As<IUser>();
                LoyalzooUserPart loyalzooPart = context.ContentItem.As<LoyalzooUserPart>();

                if (userPart != null && loyalzooPart != null)
                {
                    return CreateLoyalzooAccount(loyalzooPart, userPart.UserName, userPart.Email);

                    //string userName = "";
                    //if (userPart.UserName == "testale2")
                    //    userName = Guid.NewGuid().ToString();
                    //else
                    //    userName = userPart.UserName;

                    //return CreateLoyalzooAccount(loyalzooPart, userName, userPart.Email);
                }
                else
                    return new APIResult { success = false, data = null, message = "The user is not configured to use Loyalzoo." };
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult CreateLoyalzooAccount(LoyalzooUserPart loyalzooPart, string username, string email)
        {
            try
            {
                APIResult result = new APIResult();

                if (loyalzooPart != null && !String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(email))
                {
                    if (String.IsNullOrWhiteSpace(loyalzooPart.LoyalzooUsername) && String.IsNullOrWhiteSpace(loyalzooPart.LoyalzooPassword) && String.IsNullOrWhiteSpace(loyalzooPart.CustomerSessionId))
                    {
                        ConfigEnv configData = GetConfigData();

                        CDataCustomer customerData = new CDataCustomer();
                        customerData.first_name = username;
                        customerData.email = email;
                        customerData.username = username;
                        customerData.password = Membership.GeneratePassword(12,4);

                        ILoyalzooCustomer customer = new Customer();
                        CResultCreate creationRequest = customer.Create(configData, customerData);

                        if (creationRequest.success)
                        {
                            loyalzooPart.LoyalzooUsername = creationRequest.response.username;
                            loyalzooPart.CustomerSessionId = creationRequest.response.session_id;
                            loyalzooPart.LoyalzooPassword = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(customerData.password)));

                            result.success = true;
                            result.data = creationRequest.response;
                            result.message = "";
                        }
                        else
                        {
                            result.success = false;
                            result.message = creationRequest.Errore.response;
                            result.data = null;
                        }
                    }
                    else
                        return new APIResult { success = false, data = null, message = "There is already some Loyalzoo data associated to the user. If you want to register a new account, delete the existing Loyalzoo data and then call this method again (any previous data associated to the user will be lost)." };
                }
                else
                    return new APIResult { success = false, data = null, message = "The user is not configured to use Loyalzoo." };

                return result;
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult GetCustomerDetails()
        {
            try
            {
                APIResult result = new APIResult();

                APIResult customerIdRequest = GetCustomerSessionId();

                if (customerIdRequest.success)
                {
                    CResultCreate customerData = GetCustomerData((string)customerIdRequest.data);
                    if (customerData.success)
                    {
                        Dictionary<string, int> placeList = new Dictionary<string, int>();

                        if (customerData.response.rewards != null)
                        {
                            foreach (KeyValuePair<string, int> item in customerData.response.rewards)
                            {
                                MResultPlace placeRequest = GetPlaceData(item.Key);
                                if (placeRequest.success)
                                    placeList.Add(placeRequest.response.name, item.Value);
                            }
                        }

                        customerData.response.rewards = placeList;

                        result.success = true;
                        result.message = "";
                        result.data = customerData.response;
                    }
                    else
                    {
                        result.success = false;
                        result.message = customerData.Errore.response;
                        result.data = null;
                    }
                }
                else
                {
                    result.success = false;
                    result.message = customerIdRequest.message;
                    result.data = null;
                }

                return result;
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult GetPlaceData()
        {
            try
            {
                APIResult result = new APIResult();

                MerchantApiData merchantData = GetMerchantApiData();
                MResultPlace placeRequest = GetPlaceData(merchantData.PlaceId);

                if (placeRequest.success)
                {
                    result.success = true;
                    result.message = "";
                    result.data = placeRequest.response;
                }
                else
                {
                    result.success = false;
                    result.message = placeRequest.Errore.response;
                    result.data = null;
                }

                return result;
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult AddPoints(int numPoints)
        {
            try
            {
                APIResult result = new APIResult();

                APIResult customerIdRequest = GetCustomerSessionId();

                if (customerIdRequest.success)
                {
                    CResultCreate customerData = GetCustomerData((string)customerIdRequest.data);

                    if (customerData.success)
                    {
                        string customerId = customerData.response.id.ToString();

                        MerchantApiData merchantData = GetMerchantApiData();

                        if (merchantData.Success)
                        {
                            ConfigEnv configData = GetConfigData();

                            MgivePoint givePoints = new MgivePoint();
                            givePoints.customer_id = customerId;
                            givePoints.place_id = merchantData.PlaceId;
                            givePoints.session_id = merchantData.MerchantId;
                            givePoints.amount = numPoints.ToString();

                            ILoyalzooMerchant m = new Merchant();
                            MResultGeneral loyalzooResult = m.givePointsFromAmount(configData, givePoints);

                            if (loyalzooResult.success)
                            {
                                result.success = true;
                                result.message = "";
                                result.data = null;
                            }
                            else
                            {
                                result.success = false;
                                result.message = loyalzooResult.Errore.response;
                                result.data = null;
                            }
                        }
                        else
                        {
                            result.success = false;
                            result.message = merchantData.ErrorMessage;
                            result.data = null;
                        }
                    }
                    else
                    {
                        result.success = false;
                        result.message = customerData.Errore.response;
                        result.data = null;
                    }
                }
                else
                {
                    result.success = false;
                    result.message = customerIdRequest.message;
                    result.data = null;
                }

                return result;
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult AddPointsFromAction(string actionId, string completionPercent)
        {
            try
            {
                APIResult result = new APIResult();

                APIResult customerIdRequest = GetCustomerSessionId();

                if (customerIdRequest.success)
                {
                    CResultCreate customerData = GetCustomerData((string)customerIdRequest.data);

                    if (customerData.success)
                    {
                        string customerId = customerData.response.id.ToString();

                        MerchantApiData merchantData = GetMerchantApiData();

                        if (merchantData.Success)
                        {
                            ConfigEnv configData = GetConfigData();

                            MgivePoint givePoints = new MgivePoint();
                            givePoints.customer_id = customerId;
                            givePoints.place_id = merchantData.PlaceId;
                            givePoints.session_id = merchantData.MerchantId;
                            givePoints.amount = "0";
                            givePoints.actionid = actionId;

                            ILoyalzooMerchant m = new Merchant();
                            MResultGivePointsFromAction loyalzooResult = m.givePointsFromAction(configData, givePoints);

                            if (loyalzooResult.success)
                            {
                                result.success = true;
                                result.message = "";
                                result.data = new { pointsAdded = loyalzooResult.response.points_given, customerTotalPoints = loyalzooResult.response.balance };
                            }
                            else
                            {
                                result.success = false;
                                result.message = loyalzooResult.Errore.response;
                                result.data = null;
                            }
                        }
                        else
                        {
                            result.success = false;
                            result.message = merchantData.ErrorMessage;
                            result.data = null;
                        }
                    }
                    else
                    {
                        result.success = false;
                        result.message = customerData.Errore.response;
                        result.data = null;
                    }
                }
                else
                {
                    result.success = false;
                    result.message = customerIdRequest.message;
                    result.data = null;
                }

                return result;
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult UpdateSocial(string token, string tokenType)
        {
            try
            {
                APIResult result = new APIResult();

                APIResult customerIdRequest = GetCustomerSessionId();

                if (customerIdRequest.success)
                {
                    ConfigEnv configData = GetConfigData();

                    CUpdateSocial updateSocial = new CUpdateSocial();
                    updateSocial.Csession_id = (string)customerIdRequest.data;
                    updateSocial.social_token = token;
                    updateSocial.social_type = tokenType;

                    ILoyalzooCustomer c = new Customer();
                    CResultOperation loyalzooResult = c.CUpdateSocial(configData, updateSocial);

                    if (loyalzooResult.success)
                    {
                        result.success = true;
                        result.message = "";
                        result.data = null;
                    }
                    else
                    {
                        result.success = false;
                        result.message = loyalzooResult.Errore.response;
                        result.data = null;
                    }
                }
                else
                {
                    result.success = false;
                    result.message = customerIdRequest.message;
                    result.data = null;
                }

                return result;
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        public APIResult GiveReward(string rewardId)
        {
            try
            {
                APIResult result = new APIResult();

                APIResult customerIdRequest = GetCustomerSessionId();

                if (customerIdRequest.success)
                {
                    CResultCreate customerData = GetCustomerData((string)customerIdRequest.data);

                    if (customerData.success)
                    {
                        MerchantApiData merchantData = GetMerchantApiData();

                        if (merchantData.Success)
                        {
                            string customerId = customerData.response.id.ToString();
                            int customerPoints = customerData.response.rewards[merchantData.PlaceId];

                            ConfigEnv configData = GetConfigData();

                            MgiveReward rewardData = new MgiveReward();
                            rewardData.customer_id = customerData.response.id.ToString();
                            rewardData.place_id = merchantData.PlaceId;
                            rewardData.session_id = merchantData.MerchantId;
                            rewardData.reward_id = rewardId;
                            rewardData.punteggio = customerData.response.rewards[merchantData.PlaceId];

                            ILoyalzooMerchant m = new Merchant();
                            MResultGeneral giveRewardResponse = m.giveReward(configData, rewardData);

                            if (giveRewardResponse.success)
                            {
                                result.success = true;
                                result.message = "";
                                result.data = null;
                            }
                            else
                            {
                                result.success = false;
                                result.message = giveRewardResponse.Errore.response;
                                result.data = null;
                            }
                        }
                        else
                        {
                            result.success = false;
                            result.message = merchantData.ErrorMessage;
                            result.data = null;
                        }
                    }
                    else
                    {
                        result.success = false;
                        result.message = customerData.Errore.response;
                        result.data = null;
                    }
                }
                else
                {
                    result.success = false;
                    result.message = customerIdRequest.message;
                    result.data = null;
                }

                return result;
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }

        private ConfigEnv GetConfigData()
        {
            var fidelitySettings = _orchardServices.WorkContext.CurrentSite.As<FidelitySiteSettingsPart>();

            ConfigEnv configData = new ConfigEnv();
            configData.UrlAPI = fidelitySettings.ApiURL;
            configData.SKEY = fidelitySettings.DeveloperKey;

            return configData;
        }

        private CResultCreate GetCustomerData(string customerSessionId)
        {
            try
            {
                ConfigEnv configData = GetConfigData();

                CDataCustomer customer = new CDataCustomer();
                customer.session_id = customerSessionId;

                ILoyalzooCustomer c = new Customer();
                CResultCreate loyalzooResult = c.Me(configData, customer);

                return loyalzooResult;
            }
            catch (Exception e)
            {
                CResultCreate exceptionData = new CResultCreate();
                exceptionData.success = false;
                exceptionData.response = null;
                exceptionData.Errore = new Errore { success = false, response = e.Message };

                return exceptionData;
            }
        }

        public MerchantApiData GetMerchantApiData()
        {
            try
            {
                var fidelitySettings = _orchardServices.WorkContext.CurrentSite.As<FidelitySiteSettingsPart>();

                MerchantApiData result = new MerchantApiData();
                result.Success = true;

                if (String.IsNullOrWhiteSpace(fidelitySettings.MerchantSessionId) || String.IsNullOrWhiteSpace(fidelitySettings.PlaceId))
                {
                    ConfigEnv configData = GetConfigData();

                    MLogin merchantLogin = new MLogin();
                    merchantLogin.username = fidelitySettings.MerchantUsername;
                    merchantLogin.password = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(fidelitySettings.MerchantPwd)));

                    ILoyalzooMerchant m = new Merchant();
                    MResultLogin loginResult = m.Login(configData, merchantLogin);

                    if (loginResult.success)
                    {
                        fidelitySettings.MerchantSessionId = loginResult.response.session_id;
                        fidelitySettings.PlaceId = loginResult.response.place_id;
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = loginResult.Errore.response;
                    }
                }

                if (result.Success)
                {
                    result.MerchantId = (string)fidelitySettings.MerchantSessionId;
                    result.PlaceId = (string)fidelitySettings.PlaceId;
                    result.ErrorMessage = "";
                }
                else
                {
                    result.MerchantId = result.PlaceId = "";
                }

                return result;
            }
            catch (Exception ex)
            {
                return new MerchantApiData
                {
                    MerchantId = "",
                    PlaceId = "",
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private MResultPlace GetPlaceData(string placeId)
        {
            try
            {
                ConfigEnv configData = GetConfigData();

                MerchantApiData merchantData = GetMerchantApiData();
                MPlace place = new MPlace();
                place.session_id = merchantData.MerchantId;
                place.place_id = placeId;

                ILoyalzooMerchant m = new Merchant();
                MResultPlace placeData = m.Place(configData, place);

                return placeData;
            }
            catch (Exception e)
            {
                MResultPlace exceptionData = new MResultPlace();
                exceptionData.success = false;
                exceptionData.response = null;
                exceptionData.Errore = new Errore { success = false, response = e.Message };

                return exceptionData;
            }
        }

        private APIResult GetCustomerSessionId()
        {
            try
            {
                APIResult result = new APIResult();

                var authenticatedUser = _authenticationService.GetAuthenticatedUser();

                if (authenticatedUser != null)
                {
                    dynamic loyalzooPart = authenticatedUser.ContentItem.As<LoyalzooUserPart>();

                    if (loyalzooPart != null)
                    {
                        if (!String.IsNullOrWhiteSpace(loyalzooPart.CustomerSessionId))
                        {
                            result.success = true;
                            result.message = "";
                            result.data = loyalzooPart.CustomerSessionId;

                            return result;
                        }
                        else if (!String.IsNullOrWhiteSpace(loyalzooPart.LoyalzooUsername) && !String.IsNullOrWhiteSpace(loyalzooPart.LoyalzooPassword))
                        {
                            ConfigEnv configData = GetConfigData();
                            ILoyalzooCustomer customer = new Customer();

                            CLogin loginData = new CLogin();
                            loginData.username = loyalzooPart.LoyalzooUsername;
                            loginData.password = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(loyalzooPart.LoyalzooPassword)));
                            CResultLogin loginRequest = customer.Login(configData, loginData);

                            if (loginRequest.success)
                            {
                                loyalzooPart.CustomerSessionId = loginRequest.response.session_id;

                                result.success = true;
                                result.message = "";
                                result.data = loginRequest.response.session_id;

                                return result;
                            }
                            else
                                return new APIResult { success = false, data = null, message = loginRequest.Errore.response };
                        }
                        else if (!String.IsNullOrWhiteSpace(loyalzooPart.LoyalzooUsername) || !String.IsNullOrWhiteSpace(loyalzooPart.LoyalzooPassword))
                            return new APIResult { success = false, data = null, message = "Cannot authenticate the user. The Loyalzoo data associated with the user is incomplete." };
                        else
                            return new APIResult { success = false, data = null, message = "The user is not associated to any Loyalzoo data. Please call the LoyalzooRegistration method first." };
                    }
                    else
                        return new APIResult { success = false, data = null, message = "The user is not configured to use Loyalzoo." };
                }
                else
                    return new APIResult { success = false, data = null, message = "Invalid cookie." };
            }
            catch (Exception e)
            {
                APIResult exceptionData = new APIResult();

                exceptionData.success = false;
                exceptionData.message = e.Message;
                exceptionData.data = null;

                return exceptionData;
            }
        }
    }
}