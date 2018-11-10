using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.FidelityGateway.Models;
using Laser.Orchard.FidelityGateway.ViewModels;
using Laser.Orchard.FidelityGateway.Services;
using Orchard.Data;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.FidelityGateway.Controllers
{
    public class ActionCampaignController : Controller
    {
        protected readonly IRepository<ActionInCampaignRecord> _actionRepository;
        protected readonly IFidelityServices _fidelityService;
        protected readonly IOrchardServices _orchardServices;
        protected readonly FidelitySettingsPart settingsPart;

        public ActionCampaignController(IRepository<ActionInCampaignRecord> repository, IEnumerable<IFidelityServices> services, IOrchardServices orchardServ)
        {
            _actionRepository = repository;
            if (services.Count() > 0)
            {
                _fidelityService = services.OrderBy(a => a.GetProviderName()).ToList()[0];
            }          
            _orchardServices = orchardServ;
            settingsPart = _orchardServices.WorkContext.CurrentSite.As<FidelitySettingsPart>();
        }

        [Admin]
        public ActionResult Index()
        {
            ActionCampaign model = new ActionCampaign();
            model.actions = _actionRepository.Table.ToList();
            model.usedProvider = _fidelityService.GetProviderName();
            return View("ActionCampaign/Index", model);
        }

        [Admin]
        public ActionResult Delete(int id)
        {
            ActionInCampaignRecord action = _actionRepository.Get(a => a.Id == id);
            if (action != null)
            {
                _actionRepository.Delete(action);
            }
            return Index();
        }

        [Admin]
        public ActionResult Edit(int id)
        {         // TODO associare lista campagne disponibili per la scelta della campagna
            EditAction model;
            if (id != 0)
            {
                ActionInCampaignRecord action = _actionRepository.Get(a => a.Id == id);
                if (action != null)
                {
                    model = new EditAction()
                    {
                        Id = action.Id,
                        Action = action.Action,
                        CampaignId = action.CampaignId,
                        Points = action.Points,
                        Provider = action.Provider
                    };
                }
                else
                {
                    return Index();
                }
            }
            else
            {
                model = new EditAction();
                model.Provider = _fidelityService.GetProviderName();
            }
            APIResult<IEnumerable<FidelityCampaign>> res = _fidelityService.GetCampaignList();
            if (res.success)
            {
                model.CampaignList = new List<string>();
                foreach (FidelityCampaign camp in res.data)
                {
                    model.CampaignList.Add(camp.Id);
                }
            }
            return View("ActionCampaign/Edit", model);
        }

        [Admin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(EditAction editedAction)
        {
            ActionInCampaignRecord action;
            if (editedAction.Id != 0)
            {
                action = _actionRepository.Get(a => a.Id == editedAction.Id);
                if (action == null)
                {
                    return Index();
                }
            }
            else
            {
                action = new ActionInCampaignRecord();
                if (settingsPart.DefaultCampaign != null)
                {
                    action.CampaignId = settingsPart.DefaultCampaign;
                }
                _actionRepository.Create(action);
            }

            action.Action = editedAction.Action;
            action.CampaignId = editedAction.CampaignId;
            action.Provider = editedAction.Provider;
            action.Points = editedAction.Points;
            return Index();
        }
    }
}