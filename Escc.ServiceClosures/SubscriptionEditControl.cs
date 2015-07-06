using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using eastsussexgovuk.webservices.FormControls;
using System.Globalization;
using eastsussexgovuk.webservices.FormControls.Validators;
using EsccWebTeam.FormControls.Validators;
using System.ComponentModel;
using System.Web.UI.HtmlControls;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Control for adding/editing reasons for service closure subscriptions
    /// </summary>
    public class SubscriptionEditControl : WebControl, INamingContainer
    {
        #region Fields

        HtmlInputHidden serviceId = new HtmlInputHidden();
        HtmlInputHidden subscriptionId = new HtmlInputHidden();
        TextBox address = new TextBox();
        DropDownList type = new DropDownList();
        CheckBox global = new CheckBox();
        TextBox code = new TextBox();
        CheckBox official = new CheckBox();
        CheckBox active = new CheckBox();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEditControl"/> class.
        /// </summary>
        public SubscriptionEditControl()
            : base(HtmlTextWriterTag.Div)
        {
            this.SaveButtonText = "Save";
            this.ServiceType = new ServiceType();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the service which may be subscribed to
        /// </summary>
        /// <value>The service.</value>
        public Service Service { get; set; }

        /// <summary>
        /// Gets or sets the closure subscription to edit
        /// </summary>
        /// <value>The subscription.</value>
        public Subscription Subscription { get; set; }

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
        /// Gets or sets the type of service to subscribe to.
        /// </summary>
        /// <value>The service type.</value>
        public ServiceType ServiceType { get; set; }

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
            if (this.Service == null || this.ServiceType == null) return;

            // Display any errors to user
            this.Controls.Add(new EsccValidationSummary());

            // Add header template
            if (HeaderTemplate != null)
            {
                XhtmlContainer header = new XhtmlContainer();
                HeaderTemplate.InstantiateIn(header);
                this.Controls.Add(header);
            }

            // Remember the service
            serviceId.ID = "serviceId";
            if (!this.Page.IsPostBack) serviceId.Value = Service.Id.ToString(CultureInfo.CurrentCulture);
            this.Controls.Add(serviceId);

            // Remember the id
            subscriptionId.ID = "subscriptionId";
            if (!this.Page.IsPostBack && this.Subscription != null) subscriptionId.Value = Subscription.Id.ToString(CultureInfo.CurrentCulture);
            this.Controls.Add(subscriptionId);

            // Address
            address.ID = "address";
            address.MaxLength = 255;
            if (!this.Page.IsPostBack && this.Subscription != null) address.Text = this.Subscription.Address;
            FormPart addressPart = new FormPart("Address", address);
            addressPart.Required = true;
            this.Controls.Add(addressPart);

            // Type 
            type.ID = "type";

            ListItem emailItem = new ListItem("Email", Convert.ToInt32(SubscriptionType.Email).ToString(CultureInfo.CurrentCulture));
            if (!this.Page.IsPostBack && this.Subscription != null && this.Subscription.Type == SubscriptionType.Email) emailItem.Selected = true;
            type.Items.Add(emailItem);

            ListItem smsItem = new ListItem("Text message", Convert.ToInt32(SubscriptionType.TextMessage).ToString(CultureInfo.CurrentCulture));
            if (!this.Page.IsPostBack && this.Subscription != null && this.Subscription.Type == SubscriptionType.TextMessage) smsItem.Selected = true;
            type.Items.Add(smsItem);

            FormPart typePart = new FormPart("Type", type);
            typePart.Required = true;
            this.Controls.Add(typePart);

            // All services
            global.ID = "global";
            if (!this.Page.IsPostBack && this.Subscription != null) global.Checked = this.Subscription.Global;
            global.Text = "All " + this.ServiceType.PluralText;

            HtmlGenericControl globalPart = new HtmlGenericControl("div");
            globalPart.Attributes["class"] = "formPart";
            HtmlGenericControl globalContainer = new HtmlGenericControl("div");
            globalContainer.Attributes["class"] = "formControl noLabel";
            globalContainer.Controls.Add(global);
            globalPart.Controls.Add(globalContainer);
            this.Controls.Add(globalPart);

            // Official notification
            official.ID = "official";
            if (!this.Page.IsPostBack && this.Subscription != null) official.Checked = this.Subscription.OfficialNotification;
            official.Text = "Use official notification format";

            HtmlGenericControl officialPart = new HtmlGenericControl("div");
            officialPart.Attributes["class"] = "formPart";
            HtmlGenericControl officialContainer = new HtmlGenericControl("div");
            officialContainer.Attributes["class"] = "formControl noLabel";
            officialContainer.Controls.Add(official);
            officialPart.Controls.Add(officialContainer);
            this.Controls.Add(officialPart);

            // Code
            code.ID = "code";
            code.MaxLength = 36;
            if (!this.Page.IsPostBack && this.Subscription != null && this.Subscription.Code != null && this.Subscription.Code != new Guid())
            {
                code.Text = this.Subscription.Code.ToString();
            }
            FormPart codePart = new FormPart("Activation code", code);
            this.Controls.Add(codePart);

            // Active
            active.ID = "active";
            if (!this.Page.IsPostBack && this.Subscription != null) active.Checked = this.Subscription.Active;
            active.Text = "Active";

            HtmlGenericControl activePart = new HtmlGenericControl("div");
            activePart.Attributes["class"] = "formPart";
            HtmlGenericControl activeContainer = new HtmlGenericControl("div");
            activeContainer.Attributes["class"] = "formControl noLabel";
            activeContainer.Controls.Add(active);
            activePart.Controls.Add(activeContainer);
            this.Controls.Add(activePart);

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
                if (this.Subscription != null)
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
            // Check the address is specified
            EsccRequiredFieldValidator reqAddress = new EsccRequiredFieldValidator(address.ID, "Please enter the subscription address");
            this.Controls.Add(reqAddress);

            // Checks the address is not too long
            LengthValidator lenAddress = new LengthValidator(address.ID, "Your subscription address must be no longer than 255 characters", 0, 255);
            this.Controls.Add(lenAddress);

            // Check address characters are valid
            EsccCustomValidator charsAddress = new EsccCustomValidator(address.ID, "Please use only letters, numbers and punctuation for the subscription address");
            charsAddress.ServerValidate += new ServerValidateEventHandler(charsAddress_ServerValidate);
            this.Controls.Add(charsAddress);

            // Check code is a GUID
            EsccCustomValidator codeValidator = new EsccCustomValidator(code.ID, "Subscription code must be 32 characters and 4 dashes");
            codeValidator.ServerValidate += new ServerValidateEventHandler(codeValidator_ServerValidate);
            this.Controls.Add(codeValidator);
        }

        /// <summary>
        /// Handles the ServerValidate event of the codeValidator control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void codeValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {
                Guid guid = new Guid(code.Text);
                args.IsValid = true;
            }
            catch (FormatException)
            {
                args.IsValid = false;
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the charsAddress control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        void charsAddress_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;

            for (int i = 0; i < address.Text.Length; i++)
            {
                if (!(char.IsLetterOrDigit(address.Text, i) | char.IsPunctuation(address.Text, i) | char.IsWhiteSpace(address.Text, i)))
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
            if (this.Page.IsValid) this.OnSubscriptionSubmitted();
        }

        /// <summary>
        /// Occurs when a valid closure subscription is submitted.
        /// </summary>
        public event EventHandler<ClosureEventArgs> SubscriptionSubmitted;

        /// <summary>
        /// Called when a valid closure subscription is submitted.
        /// </summary>
        protected virtual void OnSubscriptionSubmitted()
        {
            // Gather subscription data from the validated controls, and fire the SubscriptionSubmitted event
            if (this.SubscriptionSubmitted != null)
            {
                ClosureEventArgs e = new ClosureEventArgs();

                e.Subscription = new Subscription();
                if (subscriptionId.Value.Length > 0) e.Subscription.Id = Convert.ToInt32(subscriptionId.Value);
                e.Subscription.Address = address.Text;
                e.Subscription.Type = (SubscriptionType)Enum.Parse(typeof(SubscriptionType), type.SelectedValue);
                if (!global.Checked)
                {
                    e.Subscription.Service = new Service();
                    e.Subscription.Service.Id = Int32.Parse(serviceId.Value);
                }
                if (code.Text.Length > 0) e.Subscription.Code = new Guid(code.Text);
                e.Subscription.OfficialNotification = official.Checked;
                e.Subscription.Active = active.Checked;

                this.SubscriptionSubmitted(this, e);
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
