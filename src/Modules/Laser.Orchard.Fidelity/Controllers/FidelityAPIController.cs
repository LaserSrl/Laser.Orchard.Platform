using Laser.Orchard.Fidelity.Services;
using Laser.Orchard.Fidelity.ViewModels;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Fidelity.Controllers
{
    public class FidelityAPIController : Controller
    {
        private readonly IFidelityService _fidelityService;

        public FidelityAPIController(IFidelityService fidelityService)
        {
            _fidelityService = fidelityService;
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult LoyalzooRegistration()
        {
            try
            {
                return Json(_fidelityService.CreateLoyalzooAccountFromCookie(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult CustomerDetails()
        {
            try
            {
                return Json(_fidelityService.GetCustomerDetails(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult PlaceData()
        {
            try
            {
                return Json(_fidelityService.GetPlaceData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult AddPoints(string numPoints)
        {
            try
            {
                int points;
                if (Int32.TryParse(numPoints, out points))
                    return Json(_fidelityService.AddPoints(points), JsonRequestBehavior.AllowGet);
                else
                    return Json(new APIResult { success = false, data = null, message = "The input parameters is not in the correct format." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult AddPointsFromAction(string actionId, string completionPercent)
        {
            try
            {
                return Json(_fidelityService.AddPointsFromAction(actionId, completionPercent), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult GiveReward(string rewardId)
        {
            try
            {
                if (rewardId != null)
                    return Json(_fidelityService.GiveReward(rewardId), JsonRequestBehavior.AllowGet);
                else
                    return Json(new APIResult { success = false, data = null, message = "The input parameters is not in the correct format." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public JsonResult UpdateSocial(string socialToken, string tokenType)
        {
            try
            {
                if (socialToken != null && tokenType != null)
                    return Json(_fidelityService.UpdateSocial(socialToken, tokenType), JsonRequestBehavior.AllowGet);
                else
                    return Json(new APIResult { success = false, data = null, message = "The input parameters is not in the correct format." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}