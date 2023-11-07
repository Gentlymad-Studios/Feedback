using System.Collections.Generic;

namespace Feedback {
    public class TaskModels {

        #region Asana Models
        public class AsanaTaskModel {
            public string gid {
                get; set;
            }
            public List<CustomField> custom_fields {
                get; set;
            }
            public string name {
                get; set;
            }
            public string notes {
                get; set;
            }
            public string project_id {
                get; set;
            }
        }

        public class CustomField {
            public string gid {
                get; set;
            }
            public string name {
                get; set;
            }
            public object display_value {
                get; set;
            }

        }
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