using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.NewsLetters.Services;
using Laser.Orchard.NewsLetters.ViewModels;
using Orchard;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Localization;
using Orchard.DisplayManagement;
using Orchard.ContentManagement;
using Orchard.Mvc;

namespace Laser.Orchard.NewsLetters.Controllers {
    public class SubscriptionController : Controller {
        private readonly INewsletterServices _newsletterServices;
        private readonly IOrchardServices _orchardServices;

        public SubscriptionController(INewsletterServices newsletterServices, IOrchardServices orchardServices, IShapeFactory shapeFactory) {
            _newsletterServices = newsletterServices;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        [HttpPost]
        [Themed]
        public ActionResult Subscribe(string name, string email, string confirmEmail, string[] subscription_Newsletters_Ids) {
            bool allOK = true;

            if (HttpContext.Request.Form["Newsletter_Subscriber_Subscribe"] == "") {
                return null;
            }
            if (email == confirmEmail && subscription_Newsletters_Ids != null && subscription_Newsletters_Ids.Length > 0) {
                foreach (var newsId in subscription_Newsletters_Ids) {
                    int newsIdValidated = 0;
                    int.TryParse(newsId, out newsIdValidated);
                    if (newsIdValidated > 0) {
                        var subscriber = new SubscriberViewModel {
                            Email = email,
                            Name = name,
                            NewsletterDefinition_Id = newsIdValidated,
                        };
                        var subRecord = _newsletterServices.TryRegisterSubscriber(subscriber);
                        allOK = allOK && subRecord != null;
                        
                        try 
                        {
                            ValidateModel(subRecord);
                        } 
                        catch (Exception) {
                            allOK = false;
                            var errors = ModelState.Values.Where(w => w.Errors.Count() > 0);
                            foreach (var error in errors) {
                                _orchardServices.Notifier.Error(T(error.Errors[0].ErrorMessage));
                            }
                        }
                    }
                }
            } 
            else {
                if (subscription_Newsletters_Ids == null || subscription_Newsletters_Ids.Length == 0) {
                    _orchardServices.Notifier.Error(T("Please select at least one newsletter!"));
                } else if (email != confirmEmail) {
                    _orchardServices.Notifier.Error(T("Email and email confirmation must match!"));
                }
                allOK = false;
            }


            if (allOK) {
                dynamic viewModel = Shape.Subscription_ConfirmationEmailSent().Subscriber(new SubscriberViewModel { Name = name, Email = email });

                viewModel.Metadata.Alternates.Add("Subscription.ConfirmationEmailSent");
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return new ShapeResult(this, viewModel);
            } 
            else {
                dynamic viewModel = Shape.Subscription_SubscribeError().Subscriber(new SubscriberViewModel { Name = name, Email = email });
                viewModel.Metadata.Alternates.Add("Subscription.SubscribeError");
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return new ShapeResult(this, viewModel);
            }

        }

        [Themed]
        public ActionResult ConfirmSubscribe(string key) {
            var subRecord = _newsletterServices.TryRegisterConfirmSubscriber(key);

            try 
            {
                ValidateModel(subRecord);
            } 
            catch (Exception ex) {
                ModelState.AddModelError("SubscriberError", ex);
            }

            if (subRecord != null) {
                _orchardServices.Notifier.Information(T("Subscription success"));
                return new ShapeResult(this, Shape.Subscription_ConfirmedEmail());
            } else {
                return new ShapeResult(this, Shape.Subscription_UnconfirmedEmail());
            }
        }

        [Themed, HttpGet]
        public ActionResult Unsubscribe(int newsletterId) {
            var model = _newsletterServices.GetNewsletterDefinition(newsletterId, VersionOptions.Published);
            return new ShapeResult(this, Shape.Subscription_Unsubscribe().ContentItem(model));
        }

        [Themed, HttpPost]
        public ActionResult Unsubscribe(string email, string confirmEmail, string[] subscription_Newsletters_Ids) {
            bool allOK = true;
            if (HttpContext.Request.Form["Newsletter_Subscriber_Unsubscribe"] == "") {
                return null;
            }
            if (email == confirmEmail && subscription_Newsletters_Ids != null && subscription_Newsletters_Ids.Length > 0) {
                foreach (var newsId in subscription_Newsletters_Ids) {
                    int newsIdValidated = 0;
                    int.TryParse(newsId, out newsIdValidated);
                    if (newsIdValidated > 0) {
                        var subscriber = new SubscriberViewModel {
                            Email = email,
                            NewsletterDefinition_Id = newsIdValidated,
                        };
                        var subRecord = _newsletterServices.TryUnregisterSubscriber(subscriber);
                        allOK = allOK && subRecord != null;
                    }
                }
            } else {
                if (subscription_Newsletters_Ids == null || subscription_Newsletters_Ids.Length == 0) {
                    _orchardServices.Notifier.Error(T("Please select at least one newsletter!"));
                } else if (email != confirmEmail) {
                    _orchardServices.Notifier.Error(T("Email and email confirmation must match!"));
                }
                allOK = false;
            }


            if (allOK) {
                dynamic viewModel = Shape.Subscription_UnsubscribeEmailSent().Subscriber(new SubscriberViewModel { Email = email });
                return new ShapeResult(this, viewModel);
            } else {
                dynamic viewModel = Shape.Subscription_UnsubscribeError().Subscriber(new SubscriberViewModel { Email = email });
                return new ShapeResult(this, viewModel);
            } 
        }

        [Themed]
        public ActionResult ConfirmUnsubscribe(string key) {

            var subRecord = _newsletterServices.TryUnregisterConfirmSubscriber(key);
            try 
            {
                ValidateModel(subRecord);
            } 
            catch (Exception ex) {
                ModelState.AddModelError("SubscriberError", ex);
            }

            if (subRecord != null) {
                _orchardServices.Notifier.Information(T("Unsubscription success"));
                return new ShapeResult(this, Shape.Subscription_UnsubscribedEmail());
            } else {
                return new ShapeResult(this, Shape.Subscription_UnsubscribeError());
            }
        }


    }
}
