using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.AspNet;
using System.Xml;

namespace Laser.Orchard.OpenAuthentication.Services {
    public class TwitterCustomClient : OAuthClient {
		public static readonly ServiceProviderDescription TwitterServiceDescription;

		public TwitterCustomClient(string consumerKey, string consumerSecret) 
            : this(consumerKey, consumerSecret, new AuthenticationOnlyCookieOAuthTokenManager())
		{
		}

		public TwitterCustomClient(string consumerKey, string consumerSecret, IOAuthTokenManager tokenManager) 
            : base("twitter", TwitterCustomClient.TwitterServiceDescription, new SimpleConsumerTokenManager(consumerKey, consumerSecret, tokenManager))
		{
		}

		protected override AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response)
		{
			string accessToken = response.AccessToken;
            string accessSecret = (response as ITokenSecretContainingMessage).TokenSecret; 
            string text = response.ExtraData["user_id"];
			string userName = response.ExtraData["screen_name"];
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("accesstoken", accessToken);
            dictionary.Add("secret", accessSecret);
			return new AuthenticationResult(true, base.ProviderName, text, userName, dictionary);
		}

		static TwitterCustomClient()
		{
			ServiceProviderDescription serviceProviderDescription = new ServiceProviderDescription();
            serviceProviderDescription.RequestTokenEndpoint = new MessageReceivingEndpoint("https://api.twitter.com/oauth/request_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
            serviceProviderDescription.UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://api.twitter.com/oauth/authenticate", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
            serviceProviderDescription.AccessTokenEndpoint = new MessageReceivingEndpoint("https://api.twitter.com/oauth/access_token", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
			serviceProviderDescription.TamperProtectionElements = new ITamperProtectionChannelBindingElement[]
			{
				new HmacSha1SigningBindingElement()
			};
			TwitterCustomClient.TwitterServiceDescription = serviceProviderDescription;
		}
    }
}
