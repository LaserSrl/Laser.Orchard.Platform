using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard.Security;
using Orchard.Environment.Extensions;
using Orchard.Users.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Users.Models;
using Newtonsoft.Json;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLoginNonceService : INonceService {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IUserService _userService;
        private readonly IEnumerable<IOTPDeliveryService> _deliveryServices;
        private readonly IOTPRepositoryService _otpRepositoryService;
        private readonly IMembershipService _membershipService;

        public NonceLoginNonceService(
            IWorkContextAccessor workContextAccessor,
            IUserService userService,
            IEnumerable<IOTPDeliveryService> deliveryServices,
            IOTPRepositoryService otpRepositoryService,
            IMembershipService membershipService) {

            _workContextAccessor = workContextAccessor;
            _userService = userService;
            _deliveryServices = deliveryServices;
            _otpRepositoryService = otpRepositoryService;
            _membershipService = membershipService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private int ValidityTime() {
            return _workContextAccessor.GetContext().CurrentSite.As<NonceLoginSettingsPart>().NonceMinutesValidity;
        }

        private OTPRecord NewOTP(UserPart user, Dictionary<string, string> additionalInformation) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            // use the base nonce from the IUserServices
            var nonce = _userService.CreateNonce(user, new TimeSpan(0, ValidityTime(), 0));
            string userName;
            DateTime expiration;
            // get the expiration actually assigned on the nonce
            _userService.DecryptNonce(nonce, out userName, out expiration);
            // create the OTP
            var otp = new OTPRecord {
                UserRecord = user.As<UserPart>().Record,
                Password = nonce,
                PasswordType = PasswordType.Nonce.ToString(),
                ExpirationUTCDate = expiration,
                AdditionalData = additionalInformation != null
                    ? JsonConvert.SerializeObject(additionalInformation, Formatting.Indented)
                    : string.Empty
            };
            // delete all old nonces that match the one we are creating
            var oldOtps = _otpRepositoryService.Get(user, PasswordType.Nonce.ToString());
            foreach (var old in oldOtps
                .Where(or => 
                    CompareDictionaries(
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(or.AdditionalData), 
                        additionalInformation))) {
                _otpRepositoryService.Delete(old);
            }
            // save the OTP
            return _otpRepositoryService.AddOTP(otp);
        }

        public string GenerateOTP(IUser user) {
            return GenerateOTP(user, null);
        }

        public string GenerateOTP(IUser user, Dictionary<string, string> additionalInformation) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            var otp = NewOTP(user.As<UserPart>(), additionalInformation);

            return otp.Password; 
        }

        public bool SendNewOTP(IUser user, DeliveryChannelType? channel) {
            return SendNewOTP(user, null);
        }

        public bool SendNewOTP(IUser user, Dictionary<string, string> additionalInformation, DeliveryChannelType? channel) {
            return SendNewOTP(user, additionalInformation, channel,null);
        }
        public bool SendNewOTP(IUser user, Dictionary<string, string> additionalInformation, DeliveryChannelType? channel, FlowType? flow) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            // create OTP
            var otp = NewOTP(user.As<UserPart>(), additionalInformation);

            return SendOTP(otp, user, channel,flow);
        }

        public bool SendOTP(OTPRecord otp, DeliveryChannelType? channel) {
            if (otp == null) {
                throw new ArgumentNullException("otp");
            }

            // get recipient
            var user = _membershipService.GetUser(otp.UserRecord.UserName);

            return SendOTP(otp, user, channel);
        }
        private bool SendOTP(OTPRecord otp, IUser user, DeliveryChannelType? channel) {
            return SendOTP(otp, user, channel, FlowType.Website);
        }

        private bool SendOTP(OTPRecord otp, IUser user, DeliveryChannelType? channel, FlowType? flow) {
            // Select / order delivery services
            var deliveryServices = _deliveryServices;
            if (channel != null) {
                deliveryServices = deliveryServices.Where(ds => ds.ChannelType == channel);
            }
            deliveryServices = deliveryServices.OrderByDescending(ds => ds.Priority);


            // send through the first channel that does not fail
            var success = false;
            foreach (var ds in deliveryServices) {
                success = ds.TrySendOTP(otp, user,flow);
                if (success)
                    break; // break on first success
            }

            return success;
        }




        public IUser UserFromNonce(string nonce) {
            return UserFromNonce(nonce, null);
        }

        public IUser UserFromNonce(string nonce, Dictionary<string, string> additionalInformation) {
            var otp = _otpRepositoryService.Get(nonce, PasswordType.Nonce.ToString());
            IUser user = null;
            if (otp != null) {
                // otp still valid?
                if (otp.ExpirationUTCDate >= DateTime.UtcNow) {
                    // compare additional info
                    var otpInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(otp.AdditionalData);
                    if (CompareDictionaries(otpInfo, additionalInformation)) {
                        // the information matches
                        // get recipient
                        user = _membershipService.GetUser(otp.UserRecord.UserName);
                        // otp has been used up
                        _otpRepositoryService.Delete(otp);
                    }
                } else {
                    // otp expired
                    _otpRepositoryService.Delete(otp);
                }
            }

            return user;
        }

        private bool CompareDictionaries(Dictionary<string, string> first, Dictionary<string, string> second) {
            if (NullOrEmpty(first) && NullOrEmpty(second)) {
                // both null or empty, so they are the same
                return true;
            }
            if ((NullOrEmpty(first) && !NullOrEmpty(second))
                || (!NullOrEmpty(first) && NullOrEmpty(second))) {
                // one is empty and the other is not
                return false;
            }
            // here neither dictionary is null, nor empty
            if (first.Count != second.Count) {
                return false;
            }
            // here both dictionaries have the same number of elements
            if (first.Keys.Except(second.Keys, StringComparer.InvariantCultureIgnoreCase).Any()) {
                // different keys in the two dictionaries
                return false;
            }
            // here keys are the same. Time to compare values.
            if (first.Any(entry => second[entry.Key] != entry.Value)) {
                // values are different
                return false;
            }
            // finally
            return true;
        }

        private bool NullOrEmpty(Dictionary<string, string> dictionary) {
            return dictionary == null || !dictionary.Any();
        }

        public bool ValidatePassword(OTPContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            if (context.User == null) {
                throw new ArgumentException(T("Context.User cannot be null.").Text);
            }

            //get the otp
            var valid = false;
            var otp = _otpRepositoryService.Get(context.Password, PasswordType.Nonce.ToString());
            if (otp != null) {
                if (otp.ExpirationUTCDate >= DateTime.UtcNow) {
                    // otp still valid
                    // get recipient
                    var user = _membershipService.GetUser(otp.UserRecord.UserName);
                    if (user.UserName == context.User.UserName) {
                        // same user
                        valid = true;
                        // otp has been used, so delete it
                        _otpRepositoryService.Delete(otp);
                    }
                } else {
                    // otp expired
                    _otpRepositoryService.Delete(otp);
                }
            }

            return valid;
        }

   
    }
}