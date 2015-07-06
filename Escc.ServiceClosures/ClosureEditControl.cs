using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using eastsussexgovuk.webservices.FormControls;
using eastsussexgovuk.webservices.FormControls.Validators;
using EsccWebTeam.FormControls.Validators;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Control for adding/editing service closures
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="Closure"/> property is used to determine whether a closure is being edited. 
    /// If <see cref="Closure"/> is <c>null</c> the control is set up to add a new closure.</para>
    /// <para>The <c>Enabled</c> property is used to determine whether a closure is being deleted.
    /// If so, no action buttons are shown because is would be disabled along with the other controls.
    /// SID has its own delete button.</para>
    /// </remarks>
    public class ClosureEditControl : WebControl, INamingContainer
    {
        #region Fields

        HtmlInputHidden closureId = new HtmlInputHidden();
        HtmlInputHidden serviceId = new HtmlInputHidden();
        HtmlInputHidden serviceName = new HtmlInputHidden();
        HtmlInputHidden serviceCode = new HtmlInputHidden();
        DropDownList statusList = new DropDownList();
        DateControl startDate = new DateControl();
        DateControl endDate = new DateControl();
        DropDownList reasonList = new DropDownList();
        TextBox notes = new TextBox();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClosureEditControl"/> class.
        /// </summary>
        public ClosureEditControl()
            : base(HtmlTextWriterTag.Div)
        {
            this.SaveButtonText = "Save";
            this.ShowCancelButton = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the service which may be closed
        /// </summary>
        /// <value>The service.</value>
        public Service Service { get; set; }

        /// <summary>
        /// Gets or sets the closure to edit
        /// </summary>
        /// <value>The closure.</value>
        public Closure Closure { get; set; }

        /// <summary>
        /// Gets or sets the header template which appears above the editing controls
        /// </summary>
        /// <value>The header template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio
        public ITemplate HeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the footer template which appears between the editing controls and the Save button.
        /// </summary>
        /// <value>The footer template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio
        public ITemplate FooterTemplate { get; set; }

        /// <summary>
        /// Gets or sets the save button text.
        /// </summary>
        /// <value>The save button text.</value>
        public string SaveButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show a cancel button when editing.
        /// </summary>
        /// <value><c>true</c> to show cancel button; otherwise, <c>false</c>.</value>
        public bool ShowCancelButton { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow entry of new closures in the past.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to allow closures in the past; otherwise, <c>false</c>.
        /// </value>
        public bool AllowClosuresInPast { get; set; }

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
            if (this.Service == null) return;

            // Display any errors to user
            this.Controls.Add(new EsccValidationSummary());

            // Add header template
            if (HeaderTemplate != null)
            {
                XhtmlContainer header = new XhtmlContainer();
                HeaderTemplate.InstantiateIn(header);
                this.Controls.Add(header);
            }

            // Remember the closure, if editing
            closureId.ID = "closureId";
            if (!this.Page.IsPostBack && Closure != null) closureId.Value = Closure.Id.ToString(CultureInfo.InvariantCulture);
            this.Controls.Add(closureId);

            // Remember the service
            serviceId.ID = "serviceId";
            if (!this.Page.IsPostBack) serviceId.Value = Service.Id.ToString(CultureInfo.InvariantCulture);
            this.Controls.Add(serviceId);

            serviceName.ID = "service";
            if (!this.Page.IsPostBack) serviceName.Value = Service.Name;
            this.Controls.Add(serviceName);

            serviceCode.ID = "serviceCode";
            if (!this.Page.IsPostBack) serviceCode.Value = Service.Code;
            this.Controls.Add(serviceCode);

            // Is the service closed?
            statusList.ID = "status";
            ListItem closedItem = new ListItem("closed", ((int)ClosureStatus.Closed).ToString());
            if (!this.Page.IsPostBack && this.Closure != null && this.Closure.Status == ClosureStatus.Closed) closedItem.Selected = true;
            statusList.Items.Add(closedItem);
            ListItem partlyClosedItem = new ListItem("partly closed", ((int)ClosureStatus.PartlyClosed).ToString());
            if (!this.Page.IsPostBack && this.Closure != null && this.Closure.Status == ClosureStatus.PartlyClosed) partlyClosedItem.Selected = true;
            statusList.Items.Add(partlyClosedItem);

            FormPart statusPart = new FormPart(String.Format("This {0} will be", Service.Type.SingularText.ToLower(CultureInfo.CurrentCulture)), statusList);
            statusPart.Required = true;
            this.Controls.Add(statusPart);

            // When?
            startDate.ID = "start";
            startDate.Label = "First day closed";
            startDate.FirstYear = AllowClosuresInPast ? (DateTime.Now.Year - 2) : DateTime.Now.Year;
            startDate.LastYear = (DateTime.Now.Year + 2);
            startDate.Required = true;
            startDate.ShowBlankDateOption = false;
            startDate.CssClass = "datePart";
            startDate.DayRequiredMessage = "Please select the start day";
            startDate.MonthRequiredMessage = "Please select the start month";
            startDate.YearRequiredMessage = "Please select the start year";
            if (!this.Page.IsPostBack)
            {
                if (this.Closure != null)
                {
                    if (this.Closure.StartDate.Year < startDate.FirstYear) startDate.FirstYear = this.Closure.StartDate.Year;
                    if (this.Closure.StartDate.Year > startDate.LastYear) startDate.LastYear = this.Closure.StartDate.Year;
                    startDate.SelectedDate = this.Closure.StartDate;
                }
                else
                {
                    startDate.SelectedDate = DateTime.Today;
                }
            }
            this.Controls.Add(startDate);

            endDate.ID = "end";
            endDate.Label = "Last day closed";
            endDate.FirstYear = AllowClosuresInPast ? (DateTime.Now.Year - 2) : DateTime.Now.Year;
            endDate.LastYear = (DateTime.Now.Year + 2);
            endDate.Required = true;
            endDate.ShowBlankDateOption = false;
            endDate.CssClass = "datePart";
            endDate.DayRequiredMessage = "Please select the end day";
            endDate.MonthRequiredMessage = "Please select the end month";
            endDate.YearRequiredMessage = "Please select the end year";
            if (!this.Page.IsPostBack)
            {
                if (this.Closure != null)
                {
                    if (this.Closure.EndDate.Year < endDate.FirstYear) endDate.FirstYear = this.Closure.EndDate.Year;
                    if (this.Closure.EndDate.Year > endDate.LastYear) endDate.LastYear = this.Closure.EndDate.Year;
                    endDate.SelectedDate = this.Closure.EndDate;
                }
                else
                {
                    endDate.SelectedDate = DateTime.Today;
                }
            }
            this.Controls.Add(endDate);

            // Help text to guide users into entering the right end date
            HtmlGenericControl endHelpBox = new HtmlGenericControl("div");
            endHelpBox.Attributes["class"] = "formControl";

            HtmlGenericControl endHelp = new HtmlGenericControl("p");
            endHelp.InnerText = "For emergencies, don't include days when you'd close anyway, such as weekends or holidays.";
            endHelpBox.Controls.Add(endHelp);

            this.Controls.Add(endHelpBox);

            // Why?
            reasonList.ID = "reason";
            reasonList.Items.Add(new ListItem());
            foreach (int key in Service.ReasonsForClosure.Keys)
            {
                if (!Service.ReasonsForClosure[key].Selectable) continue;
                ListItem item = new ListItem(Service.ReasonsForClosure[key].Reason, key.ToString(CultureInfo.CurrentCulture));
                if (!this.Page.IsPostBack && this.Closure != null && key == this.Closure.Reason.Id) item.Selected = true;
                reasonList.Items.Add(item);
            }
            FormPart reasonPart = new FormPart("Reason", reasonList);
            reasonPart.Required = true;
            this.Controls.Add(reasonPart);

            // Notes
            notes.TextMode = TextBoxMode.MultiLine;
            notes.ID = "notes";
            if (!this.Page.IsPostBack && this.Closure != null) notes.Text = this.Closure.Notes;
            FormPart notesPart = new FormPart("Notes", notes);
            this.Controls.Add(notesPart);

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

                // If there's a closure to edit, offer a cancel button
                if (this.Closure != null && ShowCancelButton)
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
            // Check status is specified and a valid ClosureStatus
            EsccRequiredFieldValidator reqStatus = new EsccRequiredFieldValidator(statusList.ID, String.Format("Please state whether the {0} will be closed or partly closed", Service.Type.SingularText.ToLower(CultureInfo.CurrentCulture)));
            this.Controls.Add(reqStatus);

            EsccRangeValidator rangeStatus = new EsccRangeValidator(statusList.ID, "The closure status must be a valid status code"); // message not friendly, but user should never see this
            rangeStatus.MinimumValue = "1";
            rangeStatus.MaximumValue = "2";
            rangeStatus.Type = ValidationDataType.Integer;
            this.Controls.Add(rangeStatus);

            // Check dates are sensible - dates in the past are allowed when editing a closure, but not for a new one (unless specifically allowed)
            if (this.Closure == null && !this.AllowClosuresInPast)
            {
                EsccCustomValidator startNotPast = new EsccCustomValidator(statusList.ID, "The start date cannot be in the past"); // Can't validate a DateControl, but any control id will do here to trigger validator
                startNotPast.ServerValidate += new ServerValidateEventHandler(startNotPast_ServerValidate);
                this.Controls.Add(startNotPast);

                EsccCustomValidator endNotPast = new EsccCustomValidator(statusList.ID, "The end date cannot be in the past"); // Can't validate a DateControl, but any control id will do here to trigger validator
                endNotPast.ServerValidate += new ServerValidateEventHandler(endNotPast_ServerValidate);
                this.Controls.Add(endNotPast);
            }

            EsccCustomValidator startBeforeEnd = new EsccCustomValidator(statusList.ID, "The closure cannot end before it starts"); // Can't validate a DateControl, but any control id will do here to trigger validator
            startBeforeEnd.ServerValidate += new ServerValidateEventHandler(startBeforeEnd_ServerValidate);
            this.Controls.Add(startBeforeEnd);

            // Check the reason is specified
            EsccRequiredFieldValidator reqReason = new EsccRequiredFieldValidator(reasonList.ID, String.Format("Please select why the {0} is closed", Service.Type.SingularText.ToLower(CultureInfo.CurrentCulture)));
            this.Controls.Add(reqReason);

            if (Service.ReasonsForClosure.Count > 0)
            {
                EsccRangeValidator rangeReason = new EsccRangeValidator(statusList.ID, "The reason must be a valid reason code"); // message not friendly, but user should never see this
                List<int> reasonIds = new List<int>(Service.ReasonsForClosure.Keys);
                reasonIds.Sort();
                rangeReason.MinimumValue = reasonIds[0].ToString(CultureInfo.CurrentCulture);
                rangeReason.MaximumValue = reasonIds[reasonIds.Count - 1].ToString(CultureInfo.CurrentCulture);
                rangeReason.Type = ValidationDataType.Integer;
                this.Controls.Add(rangeReason);
            }

            EsccCustomValidator otherRequiresNotes = new EsccCustomValidator(reasonList.ID, String.Format("Please state why the {0} is closed", Service.Type.SingularText.ToLower(CultureInfo.CurrentCulture)));
            otherRequiresNotes.ServerValidate += new ServerValidateEventHandler(otherRequiresNotes_ServerValidate);
            this.Controls.Add(otherRequiresNotes);

            // If the closure is an emergency, it can't be more than a week away
            EsccCustomValidator emergencyShortTerm = new EsccCustomValidator(reasonList.ID, String.Format("Emergency closures cannot be more than a week away"));
            emergencyShortTerm.ServerValidate += new ServerValidateEventHandler(emergencyShortTerm_ServerValidate);
            this.Controls.Add(emergencyShortTerm);

            // Checks if the notes are too long
            LengthValidator lenNotes = new LengthValidator(notes.ID, "Your notes must be no longer than 500 characters", 0, 500);
            this.Controls.Add(lenNotes);

        }

        /// <summary>
        /// Handles the ServerValidate event of the emergencyShortTerm control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void emergencyShortTerm_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Recognise an emergency reason and check it's not later than the limit
            if (!Service.ReasonsForClosure[Int32.Parse(this.reasonList.SelectedValue)].Emergency) args.IsValid = true;
            else args.IsValid = (this.startDate.SelectedDate <= DateTime.Today.AddDays(7));

        }

        /// <summary>
        /// Handles the ServerValidate event of the startBeforeEnd control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void startBeforeEnd_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (this.endDate.SelectedDate == DateTime.MinValue || this.startDate.SelectedDate <= this.endDate.SelectedDate);
        }

        /// <summary>
        /// Handles the ServerValidate event of the endNotPast control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void endNotPast_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (this.endDate.SelectedDate == DateTime.MinValue || this.endDate.SelectedDate >= DateTime.Today);
        }

        /// <summary>
        /// Handles the ServerValidate event of the startNotPast control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void startNotPast_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (this.endDate.SelectedDate == DateTime.MinValue || this.startDate.SelectedDate >= DateTime.Today);
        }

        /// <summary>
        /// Handles the ServerValidate event of the otherRequiresNotes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void otherRequiresNotes_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Recognise when a reason that requires an explanation is selected
            if (!Service.ReasonsForClosure[Int32.Parse(this.reasonList.SelectedValue)].RequiresNotes) args.IsValid = true;
            else args.IsValid = (notes.Text.Trim().Length > 0);
        }

        #endregion

        #region Submit closure event
        /// <summary>
        /// Handles the Click event of the saveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void saveButton_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid) this.OnClosureSubmitted();
        }

        /// <summary>
        /// Occurs when a valid closure is submitted.
        /// </summary>
        public event EventHandler<ClosureEventArgs> ClosureSubmitted;

        /// <summary>
        /// Called when a valid closure is submitted.
        /// </summary>
        protected virtual void OnClosureSubmitted()
        {
            // Gather closure data from the validated controls, and fire the ClosureSubmitted event
            if (this.ClosureSubmitted != null)
            {
                ClosureEventArgs e = new ClosureEventArgs();

                e.Service = new Service();
                e.Service.Id = Convert.ToInt32(serviceId.Value);
                e.Service.Name = serviceName.Value;
                e.Service.Code = serviceCode.Value;

                e.Closure = new Closure();
                if (!String.IsNullOrEmpty(this.closureId.Value)) e.Closure.Id = Int32.Parse(this.closureId.Value, CultureInfo.InvariantCulture);
                e.Closure.Status = (ClosureStatus)Enum.Parse(typeof(ClosureStatus), statusList.SelectedValue);
                e.Closure.StartDate = startDate.SelectedDate;
                e.Closure.EndDate = endDate.SelectedDate;
                e.Closure.Reason.Id = Int32.Parse(reasonList.SelectedValue);
                if (Service.ReasonsForClosure.ContainsKey(e.Closure.Reason.Id)) e.Closure.Reason = Service.ReasonsForClosure[e.Closure.Reason.Id];
                e.Closure.Notes = notes.Text;

                this.ClosureSubmitted(this, e);
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
