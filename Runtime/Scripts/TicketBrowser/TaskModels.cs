using System.Collections.Generic;

namespace Feedback {
    public class TaskModels {
        #region Asana Models
        public class ReportTags {
            public string gid {
                get; set;
            }
            public List<Tags> enum_options {
                get; set;
            }
        }

        public class Tags {
            public string gid {
                get; set;
            }
            public string name {
                get; set;
            }
        }
        #endregion
    }
}