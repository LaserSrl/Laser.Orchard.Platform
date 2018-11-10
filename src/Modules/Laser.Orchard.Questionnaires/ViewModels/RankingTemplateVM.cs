using Laser.Orchard.Questionnaires.Models;
using NHibernate.Transform;
using Orchard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class RankingTemplateVM {

        public Int32 Point { get; set; }
        public string Identifier { get; set; }
        public string UsernameGameCenter { get; set; }
        public TipoDispositivo Device { get; set; }
        public Int32 ContentIdentifier { get; set; }
        public string name { get; set; }
        public bool AccessSecured { get; set; }
        public DateTime RegistrationDate { get; set; }

    }



    public class DisplaRankingTemplateVM {
        public DisplaRankingTemplateVM() {
            ListRank = new List<RankingTemplateVM>();
        }
        public string Title { get; set; }
        public Int32 GameID { get; set; } //propagate game id to UI
        public string Device { get; set; }
        public List<RankingTemplateVM> ListRank { get; set; }
    }

    public class DisplayRankingTemplateVMModel {
        public dynamic Pager { get; set; }
        public DisplaRankingTemplateVM drtvm { get; set; }
    }


    public class AliasToBeanVMFromRecord : IResultTransformer {
        private readonly AliasToBeanResultTransformer aliasToBeanTransformer;
        public delegate RankingTemplateVM Del(RankingPartRecord rpr);
        private readonly Del _callback;

        public AliasToBeanVMFromRecord(Del cb) {
            this.aliasToBeanTransformer = new AliasToBeanResultTransformer(typeof(RankingPartRecord));
            _callback = cb;
        }

        public IList TransformList(IList collection) {
            return collection; // this.aliasToBeanTransformer.TransformList(collection);
        }

        public object TransformTuple(object[] tuple, string[] aliases) {
            if (aliases == null) {
                throw new ArgumentNullException("aliases");
            }
            RankingPartRecord result = (RankingPartRecord)tuple[0]; // this.aliasToBeanTransformer.TransformTuple(tuple, aliases);

            return _callback((RankingPartRecord)result);; //result;
        }
    }


}