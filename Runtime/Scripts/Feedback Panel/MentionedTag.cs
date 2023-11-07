using System;
using UnityEngine.UIElements;

namespace Feedback {
    public class MentionedTag {
        public VisualElement ui;
        public Label mentionedLbl;
        public Button removeBtn;

        public Action openDetails;
        public Action removeFromMentionedList;
        public string title;
        public string gid;
        public bool selected;

        public MentionedTag(VisualElement ui, string title, string gid, Action openDetails, Action removeFromMentionedList) {
            this.ui = ui;
            this.title = title;
            this.gid = gid;
            this.openDetails = openDetails;
            this.removeFromMentionedList = removeFromMentionedList;

            ui.AddToClassList("fixedTag");

            mentionedLbl = ui.Q("label") as Label;
            mentionedLbl.text = title;

            removeBtn = ui.Q("removeBtn") as Button;
            removeBtn.clicked += Remove;

            ui.RegisterCallback<ClickEvent>((evt) => openDetails.Invoke());
        }

        private void Remove() {
            ui.parent.Remove(ui);
            removeFromMentionedList.Invoke();
        }
    }
}