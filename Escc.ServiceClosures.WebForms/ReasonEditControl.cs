using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Globalization;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using Escc.FormControls.WebForms;
using Escc.FormControls.WebForms.Validators;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Control for adding/editing reasons for service closures
    /// </summary>
    public class ReasonEditControl : WebControl, INamingContainer
    {
        #region Fields

        HtmlInputHidden reasonId = new HtmlInputHidden();
        TextBox reasonEdit = new TextBox();
        CheckBox emergency = new CheckBox();
        CheckBox requiresNotes = new CheckBox();
        CheckBox selectable = new CheckBox();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReasonEditControl"/> class.
        /// </summary>
        public ReasonEditControl()
            : base(HtmlTextWriterTag.Div)
        {
            this.SaveButtonText = "Save";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the closure reason to edit
        /// </summary>
        /// <value>The reason.</value>
        public ClosureReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the header template which appears above the editing controls
        /// </summary>
        /// <value>The header template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio (allegedly)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio (allegedly)
        public ITemplate HeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the footer template which appears between the editing controls and the Save button.
        /// </summary>
        /// <value>The footer template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio (allegedly)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio (allegedly)
        public ITemplate FooterTemplate { get; set; }

        /// <summary>
        /// Gets or sets the save button text.
        /// </summary>
        /// <value>The save button text.</value>
        public string SaveButtonText { get; set; }

        /// <summary>
        /// Container for header and footer templates
        /// </summary>
        private class XhtmlContainer : PlaceHolder, INamingContainer { }

        #endregion

        #region Create controls

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Display any errors to user
            this.Controls.Add(new EsccValidationSummary());

            // Add header template
            if (HeaderTemplate != null)
            {
                XhtmlContainer header = new XhtmlContainer();
                HeaderTemplate.InstantiateIn(header);
                this.Controls.Add(header);
            }

            // Remember the id
            reasonId.ID = "reasonId";
            if (!this.Page.IsPostBack && this.Reason != null) reasonId.Value = Reason.Id.ToString(CultureInfo.CurrentCulture);
            this.Controls.Add(reasonId);

            // Reason
            reasonEdit.ID = "reason";
            reasonEdit.MaxLength = 50;
            if (!this.Page.IsPostBack && this.Reason != null) reasonEdit.Text = this.Reason.Reason;
            FormPart reasonPart = new FormPart("Reason", reasonEdit);
            reasonPart.Required = true;
            this.Controls.Add(reasonPart);

            // Emergency
            emergency.ID = "emergency";
            if (!this.Page.IsPostBack && this.Reason != null) emergency.Checked = this.Reason.Emergency;
            emergency.Text = "Emergency";

            HtmlGenericControl emergencyPart = new HtmlGenericControl("div");
            emergencyPart.Attributes["class"] = "formPart";
            HtmlGenericControl emergencyContainer = new HtmlGenericControl("div");
            emergencyContainer.Attributes["class"] = "formControl noLabel";
            emergencyContainer.Controls.Add(emergency);
            emergencyPart.Controls.Add(emergencyContainer);
            this.Controls.Add(emergencyPart);

            // Requires notes
            requiresNotes.ID = "requiresNotes";
            if (!this.Page.IsPostBack && this.Reason != null) requiresNotes.Checked = this.Reason.RequiresNotes;
            requiresNotes.Text = "Requires notes";

            HtmlGenericControl requiresNotesPart = new HtmlGenericControl("div");
            requiresNotesPart.Attributes["class"] = "formPart";
            HtmlGenericControl requiresNotesContainer = new HtmlGenericControl("div");
            requiresNotesContainer.Attributes["class"] = "formControl noLabel";
            requiresNotesContainer.Controls.Add(requiresNotes);
            requiresNotesPart.Controls.Add(requiresNotesContainer);
            this.Controls.Add(requiresNotesPart);

            // Selectable
            selectable.ID = "selectable";
            if (!this.Page.IsPostBack && this.Reason != null) selectable.Checked = this.Reason.Selectable;
            selectable.Text = "Selectable";

            HtmlGenericControl selectablePart = new HtmlGenericControl("div");
            selectablePart.Attributes["class"] = "formPart";
            HtmlGenericControl selectableContainer = new HtmlGenericControl("div");
            selectableContainer.Attributes["class"] = "formControl noLabel";
            selectableContainer.Controls.Add(selectable);
            selectablePart.Controls.Add(selectableContainer);
            this.Controls.Add(selectablePart);

            // Add footer template
            if (FooterTemplate != null)
            {
                XhtmlContainer footer = new XhtmlContainer();
                FooterTemplate.InstantiateIn(footer);
                this.Controls.Add(footer);
            }

            // Set up validation
            CreateValidators();

            // Buttons
            if (this.Enabled)
            {
                HtmlGenericControl buttonContainer = new HtmlGenericControl("div");
                buttonContainer.Attributes["class"] = "formButtons";
                this.Controls.Add(buttonContainer);

                EsccButton saveButton = new EsccButton();
                saveButton.Text = this.SaveButtonText;
                saveButton.CssClass = "button";
                saveButton.Click += new EventHandler(saveButton_Click);
                buttonContainer.Controls.Add(saveButton);

                // If there's a closure reason to edit, offer a cancel button
                if (this.Reason != null)
                {
                    EsccButton cancelButton = new EsccButton();
                    cancelButton.Text = "Cancel";
                    cancelButton.CssClass = "button";
                    cancelButton.Click += new EventHandler(cancelButton_Click);
                    buttonContainer.Controls.Add(cancelButton);
                }
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Creates the validators.
        /// </summary>
        private void CreateValidators()
        {
            // Check the reason is specified
            EsccRequiredFieldValidator reqReason = new EsccRequiredFieldValidator(reasonEdit.ID, "Please enter the reason text");
            this.Controls.Add(reqReason);

            // Checks the reason is not too long
            LengthValidator lenReason = new LengthValidator(reasonEdit.ID, "Your reason must be no longer than 50 characters", 0, 50);
            this.Controls.Add(lenReason);

            // Check reason characters are valid
            EsccCustomValidator charsReason = new EsccCustomValidator(reasonEdit.ID, "Please use only letters, numbers and punctuation for the reason");
            charsReason.ServerValidate += new ServerValidateEventHandler(charsReason_ServerValidate);
            this.Controls.Add(charsReason);
        }

        /// <summary>
        /// Handles the ServerValidate event of the charsReason control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void charsReason_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;

            for (int i = 0; i < reasonEdit.Text.Length; i++)
            {
                if (!(char.IsLetterOrDigit(reasonEdit.Text, i) | char.IsPunctuation(reasonEdit.Text, i) | char.IsWhiteSpace(reasonEdit.Text, i)))
                {
                    args.IsValid = false;
                    return;
                }
            }
        }

        #endregion

        #region Submit event
        /// <summary>
        /// Handles the Click event of the saveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void saveButton_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid) this.OnReasonSubmitted();
        }

        /// <summary>
        /// Occurs when a valid closure reason is submitted.
        /// </summary>
        public event EventHandler<ClosureEventArgs> ReasonSubmitted;

        /// <summary>
        /// Called when a valid closure is submitted.
        /// </summary>
        protected virtual void OnReasonSubmitted()
        {
            // Gather closure data from the validated controls, and fire the ReasonSubmitted event
            if (this.ReasonSubmitted != null)
            {
                ClosureEventArgs e = new ClosureEventArgs();

                e.Reason = new ClosureReason();
                if (reasonId.Value.Length > 0) e.Reason.Id = Convert.ToInt32(reasonId.Value);
                e.Reason.Reason = reasonEdit.Text;
                e.Reason.Emergency = emergency.Checked;
                e.Reason.RequiresNotes = requiresNotes.Checked;
                e.Reason.Selectable = selectable.Checked;

                this.ReasonSubmitted(this, e);
            }
        }
        #endregion

        #region Cancel event

        /// <summary>
        /// Handles the Click event of the cancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void cancelButton_Click(object sender, EventArgs e)
        {
            this.OnCancel();
        }

        /// <summary>
        /// Occurs when the cancel button is clicked
        /// </summary>
        public event EventHandler Cancel;

        /// <summary>
        /// Called when the cancel button is clicked
        /// </summary>
        protected virtual void OnCancel()
        {
            if (this.Cancel != null) this.Cancel(this, new EventArgs());
        }


        #endregion // Cancel event

    }
}
