using AutoMapper;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class EmailContactPartDriver : ContentPartDriver<EmailContactPart> {
        public Localizer T { get; set; }
        protected override string Prefix
        {
            get { return "Laser.Mobile.EmailContact"; }
        }
        private readonly IRepository<CommunicationEmailRecord> _repoEmail;
        private readonly ITransactionManager _transaction;
        private readonly IOrchardServices _orchardServices;
        private IMapper _mapper;

        public EmailContactPartDriver(IRepository<CommunicationEmailRecord> repoEmail, ITransactionManager transaction, IOrchardServices orchardServices) {
            _repoEmail = repoEmail;
            T = NullLocalizer.Instance;
            _transaction = transaction;
            _orchardServices = orchardServices;

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<CommunicationEmailRecord, View_EmailVM_element>();
                cfg.CreateMap<View_EmailVM_element, CommunicationEmailRecord>();
            });
            _mapper = mapperConfiguration.CreateMapper();
        }

        protected override DriverResult Display(EmailContactPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Detail") {
                    View_EmailVM viewModel = new View_EmailVM();
                    View_EmailVM_element vm = new View_EmailVM_element();
                    if (part.EmailEntries.Value != null) {
                        List<CommunicationEmailRecord> oldviewModel = part.EmailEntries.Value.ToList();
                        foreach (CommunicationEmailRecord cm in oldviewModel) {
                            vm = new View_EmailVM_element();
                            _mapper.Map<CommunicationEmailRecord, View_EmailVM_element>(cm, vm);
                            viewModel.Elenco.Add(vm);
                        }
                    }
                    return ContentShape("Parts_EmailContact",
                        () => shapeHelper.Parts_EmailContact(Elenco: viewModel.Elenco));
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }

        protected override DriverResult Editor(EmailContactPart part, dynamic shapeHelper) {
            View_EmailVM viewModel = new View_EmailVM();
            View_EmailVM_element vm = new View_EmailVM_element();
            // viewModel.Elenco.Add(vm);
            if (part.EmailEntries.Value != null) {
                List<CommunicationEmailRecord> oldviewModel = part.EmailEntries.Value.ToList();
                foreach (CommunicationEmailRecord cm in oldviewModel) {
                    vm = new View_EmailVM_element();
                    _mapper.Map<CommunicationEmailRecord, View_EmailVM_element>(cm, vm);
                    viewModel.Elenco.Add(vm);
                }
            }
            return ContentShape("Parts_EmailContact_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/EmailContact_Edit", Model: viewModel, Prefix: Prefix));

        }



        protected override DriverResult Editor(EmailContactPart part, IUpdateModel updater, dynamic shapeHelper) {
            View_EmailVM oldviewModel = new View_EmailVM();

            updater.TryUpdateModel(oldviewModel, Prefix, null, null);
            bool error = false;
            _transaction.Demand();
            foreach (View_EmailVM_element vmel in oldviewModel.Elenco) {
                if ((vmel.Delete || string.IsNullOrEmpty(vmel.Email)) && vmel.Id > 0) {
                    CommunicationEmailRecord cmr = _repoEmail.Fetch(x => x.Id == vmel.Id).FirstOrDefault();
                    _repoEmail.Delete(cmr);
                }
                else
                    if (!vmel.Delete) {
                    if (!string.IsNullOrEmpty(vmel.Email)) {
                        if (_repoEmail.Fetch(x => x.Email == vmel.Email && x.Id != vmel.Id).Count() > 0) {
                            error = true;
                            updater.AddModelError("Error", T("Email can't be assigned is linked to other contact"));
                        }
                    }
                    if (vmel.Id > 0) {
                        CommunicationEmailRecord cmr = _repoEmail.Fetch(x => x.Id == vmel.Id).FirstOrDefault();
                        if (cmr.Email != vmel.Email || cmr.Validated != vmel.Validated ||
                            cmr.AccettatoUsoCommerciale != vmel.AccettatoUsoCommerciale ||
                            cmr.AutorizzatoTerzeParti != vmel.AutorizzatoTerzeParti) {
                            cmr.Email = vmel.Email;
                            cmr.Validated = vmel.Validated;
                            cmr.AccettatoUsoCommerciale = vmel.AccettatoUsoCommerciale;
                            cmr.AutorizzatoTerzeParti = vmel.AutorizzatoTerzeParti;
                            cmr.DataModifica = DateTime.Now;
                            _repoEmail.Update(cmr);

                        }
                    }
                    else {
                        View_EmailVM_element vm = new View_EmailVM_element();
                        CommunicationEmailRecord cmr = new CommunicationEmailRecord();
                        _mapper.Map<View_EmailVM_element, CommunicationEmailRecord>(vm, cmr);
                        cmr.Email = vmel.Email;
                        cmr.Validated = vmel.Validated;
                        cmr.AccettatoUsoCommerciale = vmel.AccettatoUsoCommerciale;
                        cmr.AutorizzatoTerzeParti = vmel.AutorizzatoTerzeParti;
                        cmr.EmailContactPartRecord_Id = part.Id;
                        _repoEmail.Create(cmr);

                    }
                }
            }
            if (error == true)
                _transaction.Cancel();
            else
                _repoEmail.Flush();
            //    _transaction.RequireNew();
            return Editor(part, shapeHelper);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="context"></param>
        protected override void Importing(EmailContactPart part, ImportContentContext context) {
            var emailRecord = context.Data.Element(part.PartDefinition.Name).Elements("EmailRecord");
            if (emailRecord != null) {
                if (part.EmailRecord == null) {
                    part.EmailRecord = new List<CommunicationEmailRecord>();
                }
                foreach (var rec in emailRecord) {
                    string locEmail = "";
                    var Email = rec.Attribute("Email");
                    if (Email != null)
                        locEmail = Email.Value;

                    CommunicationEmailRecord recMail = part.Record.EmailRecord.FirstOrDefault(x =>
                        x.Email == locEmail);
                    if (recMail == null) {
                        recMail = new CommunicationEmailRecord();
                        recMail.EmailContactPartRecord_Id = part.Id;
                        recMail.Email = locEmail;
                        recMail.DataInserimento = DateTime.Now; //valore iniziale temporaneo per poter creare il record
                        recMail.DataModifica = DateTime.Now; //valore iniziale temporaneo per poter creare il record
                        _repoEmail.Create(recMail);
                        part.EmailRecord.Add(recMail);
                    }
                    var Language = rec.Attribute("Language");
                    if (Language != null)
                        recMail.Language = Language.Value;

                    var Validated = rec.Attribute("Validated");
                    if (Validated != null)
                        recMail.Validated = Convert.ToBoolean(Validated.Value);

                    var DataInserimento = rec.Attribute("DataInserimento");
                    if (DataInserimento != null)
                        recMail.DataInserimento = Convert.ToDateTime(DataInserimento.Value, CultureInfo.InvariantCulture);

                    var DataModifica = rec.Attribute("DataModifica");
                    if (DataModifica != null)
                        recMail.DataModifica = Convert.ToDateTime(DataModifica.Value, CultureInfo.InvariantCulture);

                    var Produzione = rec.Attribute("Produzione");
                    if (Produzione != null)
                        recMail.Produzione = Convert.ToBoolean(Produzione.Value);

                    var AccettatoUsoCommerciale = rec.Attribute("AccettatoUsoCommerciale");
                    if (AccettatoUsoCommerciale != null)
                        recMail.AccettatoUsoCommerciale = Convert.ToBoolean(AccettatoUsoCommerciale.Value);

                    var AutorizzatoTerzeParti = rec.Attribute("AutorizzatoTerzeParti");
                    if (AutorizzatoTerzeParti != null)
                        recMail.AutorizzatoTerzeParti = Convert.ToBoolean(AutorizzatoTerzeParti.Value);

                    var KeyUnsubscribe = rec.Attribute("KeyUnsubscribe");
                    if (KeyUnsubscribe != null)
                        recMail.KeyUnsubscribe = KeyUnsubscribe.Value;

                    var DataUnsubscribe = rec.Attribute("DataUnsubscribe");
                    if (DataUnsubscribe != null)
                        recMail.DataUnsubscribe = Convert.ToDateTime(DataUnsubscribe.Value, CultureInfo.InvariantCulture);
                }
                _repoEmail.Flush();
            }
        }

        protected override void Exporting(EmailContactPart part, ExportContentContext context) {
            if (part.EmailRecord != null) {
                var root = context.Element(part.PartDefinition.Name);
                foreach (CommunicationEmailRecord rec in part.EmailRecord) {
                    XElement emailText = new XElement("EmailRecord");
                    emailText.SetAttributeValue("Language", rec.Language);
                    emailText.SetAttributeValue("Validated", rec.Validated);
                    emailText.SetAttributeValue("DataInserimento", rec.DataInserimento.ToString(CultureInfo.InvariantCulture));
                    emailText.SetAttributeValue("DataModifica", rec.DataModifica.ToString(CultureInfo.InvariantCulture));
                    emailText.SetAttributeValue("Email", rec.Email);
                    emailText.SetAttributeValue("Produzione", rec.Produzione);
                    emailText.SetAttributeValue("AccettatoUsoCommerciale", rec.AccettatoUsoCommerciale);
                    emailText.SetAttributeValue("AutorizzatoTerzeParti", rec.AutorizzatoTerzeParti);
                    emailText.SetAttributeValue("KeyUnsubscribe", rec.KeyUnsubscribe);
                    if (rec.DataUnsubscribe.HasValue) {
                        emailText.SetAttributeValue("DataUnsubscribe", rec.DataUnsubscribe.Value.ToString(CultureInfo.InvariantCulture));
                    }
                    root.Add(emailText);
                }
            }
        }
    }
}