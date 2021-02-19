using AutoMapper;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using System;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Services {
    public interface IUserAgentRedirectServices : IDependency {
        void Update(ContentItem content, UserAgentRedirectEdit editModel);
        UserAgentRedirectEdit BuildEditModelForUserAgentRedirectPart(UserAgentRedirectPart part);
        MobileAppStores? GetStoreFromUserAgent();
        void PersistAnswer(string appName);
        bool RetrieveIfAnsweredYet(string appName);
    }

    public class UserAgentRedirectServices : IUserAgentRedirectServices {
        private readonly IRepository<AppStoreRedirectRecord> _appStoreRedirectRepository;
        public UserAgentRedirectServices(IRepository<AppStoreRedirectRecord> appStoreRedirectRepository) {
            _appStoreRedirectRepository = appStoreRedirectRepository;
        }

        public UserAgentRedirectEdit BuildEditModelForUserAgentRedirectPart(UserAgentRedirectPart part) {

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<AppStoreRedirectRecord, AppStoreEdit>();
                cfg.CreateMap<UserAgentRedirectPart, UserAgentRedirectEdit>()
                    .ForMember(dest => dest.Stores, opt => opt.MapFrom(src => src.Stores));
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            var viewModel = _mapper.Map<UserAgentRedirectEdit>(part);
            return (viewModel);
        }


        public void Update(ContentItem content, UserAgentRedirectEdit editModel) {

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<AppStoreEdit, AppStoreRedirectRecord>();
                cfg.CreateMap<UserAgentRedirectEdit, UserAgentRedirectPart>()
                    .ForMember(dest => dest.Stores, opt => opt.Ignore());
            });
            IMapper _mapper = mapperConfiguration.CreateMapper();

            var part = content.As<UserAgentRedirectPart>();
            _mapper.Map(editModel, part);
            var partRecord = part.Record;
            var partId = part.Id;

            // Update and Delete 
            foreach (var item in editModel.Stores.Where(w => w.Id > 0)) {
                AppStoreRedirectRecord record = _appStoreRedirectRepository.Get(item.Id);
                _mapper.Map(item, record);
                var recordQuestionID = record.Id;
                record.UserAgentRedirectPartRecord = partRecord;
                if (item.Delete) {
                    _appStoreRedirectRepository.Delete(_appStoreRedirectRepository.Get(item.Id));
                } else {
                    _appStoreRedirectRepository.Update(record);
                }
            }
            // Create
            foreach (var item in editModel.Stores.Where(w => w.Id == 0 && w.Delete == false)) {
                AppStoreRedirectRecord record = new AppStoreRedirectRecord();
                _mapper.Map(item, record);
                record.UserAgentRedirectPartRecord = partRecord;
                _appStoreRedirectRepository.Create(record);
            }

        }

        public void PersistAnswer(string appName) {
            HttpCookie myCookie = new HttpCookie("Download"+appName);
            myCookie["Answered"] = "true";
            myCookie.Expires = DateTime.Now.AddDays(1d);
            HttpContext.Current.Response.Cookies.Add(myCookie);
        }

        public bool RetrieveIfAnsweredYet(string appName) {
            return (HttpContext.Current.Request.Cookies["Download" + appName] != null);
        }

        public MobileAppStores? GetStoreFromUserAgent() {
            string strUserAgent = HttpContext.Current.Request.UserAgent.ToString().ToLower();
            if (strUserAgent.Contains("android")) {
                return MobileAppStores.GooglePlay;
            } else if (strUserAgent.Contains("iphone") || strUserAgent.Contains("ipad")) {
                return MobileAppStores.iTunes;
            } else if (strUserAgent.Contains("iemobile")) {
                return MobileAppStores.WindowsPhone;
            } else if (strUserAgent.Contains("amazon")) {
                return MobileAppStores.Amazon;
            }
            return null;

        }
    }
}