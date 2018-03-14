using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Exceptionless;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Shows today's closure information for a specific service
    /// </summary>
    public class ClosedTodayControl : WebControl
    {
        private IServiceClosureData closureData;
        private Collection<Closure> closures = null;

        /// <summary>
        /// Gets or sets the service subject to closure
        /// </summary>
        /// <value>The service.</value>
        public Service Service { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show only emergency closures.
        /// </summary>
        /// <value><c>true</c> if emergency only; otherwise, <c>false</c>.</value>
        public bool EmergencyOnly { get; set; }

        /// <summary>
        /// Gets or sets the header template which appears before the closures
        /// </summary>
        /// <value>The header template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio
        public ITemplate HeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the footer template which appears after the closures
        /// </summary>
        /// <value>The footer template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio 
        public ITemplate FooterTemplate { get; set; }

        /// <summary>
        /// Container for header and footer templates
        /// </summary>
        private class XhtmlContainer : PlaceHolder, INamingContainer { }

        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag(HtmlTextWriter writer) { }

        /// <summary>
        /// Renders the HTML closing tag of the control into the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderEndTag(HtmlTextWriter writer) { }

        /// <summary>
        /// Gets whether to check for emergency closures today or tomorrow, depending on time of day
        /// </summary>
        /// <returns></returns>
        private bool TooLateForToday()
        {
            return (DateTime.Now > DateTime.Today.Date.AddHours(16)); // change display after 4pm
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            CheckForClosures();

            // Add header template
            if (HeaderTemplate != null)
            {
                XhtmlContainer header = new XhtmlContainer();
                HeaderTemplate.InstantiateIn(header);
                this.Controls.Add(header);
            }

            // Add closure info
            if (closures.Count == 1)
            {
                // Create a paragraph
                AddClosure(closures[0], this);
            }
            else
            {
                // Create a list
                HtmlGenericControl list = new HtmlGenericControl("ol");
                list.Attributes["class"] = "closureList";
                if (!String.IsNullOrEmpty(this.CssClass)) list.Attributes["class"] = this.CssClass;
                this.Controls.Add(list);

                foreach (Closure closure in closures)
                {
                    HtmlGenericControl item = new HtmlGenericControl("li");
                    list.Controls.Add(item);

                    AddClosure(closure, item);
                }
            }

            // Add footer template
            if (FooterTemplate != null)
            {
                XhtmlContainer footer = new XhtmlContainer();
                FooterTemplate.InstantiateIn(footer);
                this.Controls.Add(footer);
            }
        }

        /// <summary>
        /// Checks for closures.
        /// </summary>
        /// <returns>Number of closures found</returns>
        /// <remarks>The control calls this anyway but making it a public method gives the host page control over when in the lifecycle it happens.</remarks>
        public int CheckForClosures()
        {
            if (closures != null) return closures.Count;

            // Read the relevant XML file
            try
            {
                closureData = new FileDataSource().ReadClosureData(this.Service.Type);
            }
            catch (FileNotFoundException ex)
            {
                // Publish the exception so that we know about it, but ensure the page
                // continues to load uninterrupted
                ex.ToExceptionless().Submit();
                this.Visible = false;
            }

            closures = (TooLateForToday()) ? closureData.ClosuresTomorrowByServiceCode(this.Service.Code, true) : closureData.ClosuresTodayByServiceCode(this.Service.Code, true);
            this.Visible = (closures.Count > 0);
            return closures.Count;
        }

        /// <summary>
        /// Adds controls listing a closure.
        /// </summary>
        /// <param name="closure">The closure.</param>
        /// <param name="container">The container.</param>
        private void AddClosure(Closure closure, Control container)
        {
            string day = (TooLateForToday()) ? "tomorrow" : "today";
            container.Controls.Add(new LiteralControl(HttpUtility.HtmlEncode(String.Format(CultureInfo.InvariantCulture, "{0} is {1} {2}, {3}", Service.Name, Regex.Replace(closure.Status.ToString(), "([A-Z])", " $1").TrimStart().ToLower(CultureInfo.CurrentCulture), day, closure.Reason.Reason))));
            if (!String.IsNullOrEmpty(closure.Notes))
            {
                container.Controls.Add(new LiteralControl("<p>" + HttpUtility.HtmlEncode(closure.Notes).Replace(Environment.NewLine + Environment.NewLine, "</p><p>") + "</p>"));
            }
        }
    }
}
