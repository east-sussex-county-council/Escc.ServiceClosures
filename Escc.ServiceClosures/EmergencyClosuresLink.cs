using System;
using System.ComponentModel;
using System.IO;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Exceptionless;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Links to a page about closed services if there are any which have been closed at short notice
    /// </summary>
    public class EmergencyClosuresLink : WebControl
    {
        private XPathClosureData closureData;
        private string cacheKey;
        private string cachedHtml;

        /// <summary>
        /// Gets or sets a word or phrase describing a single instance of the type of service subject to closure
        /// </summary>
        /// <value>The type of the service.</value>
        /// <remarks>This property is a <see cref="String"/> rather than a <see cref="ServiceType"/> so that it can be set declaritively.</remarks>
        public string ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the URL to link to.
        /// </summary>
        /// <value>The URL.</value>
        public Uri NavigateUrl { get; set; }

        /// <summary>
        /// Gets or sets the text of the link.
        /// </summary>
        /// <value>The text of the link.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the header template which appears before the link
        /// </summary>
        /// <value>The header template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio (allegedly)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio (allegedly)
        public ITemplate HeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the footer template which appears after the link
        /// </summary>
        /// <value>The footer template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio (allegedly)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio (allegedly)
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // Run the base Init event to ensure properties are read from ASPX
            base.OnInit(e);

            // Always read from cache if possible
            this.cacheKey = "Escc.ServiceClosures.EmergencyClosuresLink." + this.ServiceType;
            if (Context.Cache[cacheKey] != null)
            {
                this.cachedHtml = Context.Cache[cacheKey] as string;
                this.Visible = (!String.IsNullOrEmpty(this.cachedHtml));
            }

            if (this.cachedHtml == null)
            {
                // Otherwise read the relevant XML file
                try
                {
                    closureData = XPathClosureData.Create(new ServiceType(this.ServiceType));

                    // Set visibility early, so that it can be picked up by <see cref="EmergencyClosuresLinkContainer"/>
                    // if that is used as the parent control

                    this.Visible = (closureData != null
                        && this.NavigateUrl != null
                        && (TooLateForToday() ? closureData.EmergencyClosureExistsTomorrow() : closureData.EmergencyClosureExistsToday())); // || closureData.ClosedTodayAtShortNotice(Settings.Default.ShortNoticeDays))); // editors didn't want this to appear for short notice closures

                    // If not visible, cache that decision represented by String.Empty, because the Render method isn't even going to run
                    // and that's where caching would otherwise take place
                    Context.Cache.Insert(this.cacheKey, String.Empty, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);
                }
                catch (FileNotFoundException ex)
                {
                    // Publish the exception so that we know about it, but ensure the page
                    // continues to load uninterrupted
                    ex.ToExceptionless().Submit();
                    this.Visible = false;
                }
            }
        }

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
            // If cached, add the cached controls
            if (this.cachedHtml != null)
            {
                this.Controls.Add(new LiteralControl(this.cachedHtml));
                return;
            }

            // Otherwise build up controls
            if (this.Visible)
            {
                // Add header template
                if (HeaderTemplate != null)
                {
                    XhtmlContainer header = new XhtmlContainer();
                    HeaderTemplate.InstantiateIn(header);
                    if (TooLateForToday()) UpdateTextForTomorrow(header.Controls);
                    this.Controls.Add(header);
                }

                // Add link
                HtmlAnchor link = new HtmlAnchor();
                link.HRef = this.NavigateUrl.ToString();
                link.InnerText = this.Text;
                if (!String.IsNullOrEmpty(this.CssClass)) link.Attributes["class"] = this.CssClass;
                this.Controls.Add(link);

                // Add footer template
                if (FooterTemplate != null)
                {
                    XhtmlContainer footer = new XhtmlContainer();
                    FooterTemplate.InstantiateIn(footer);
                    if (TooLateForToday()) UpdateTextForTomorrow(footer.Controls);
                    this.Controls.Add(footer);
                }
            }
        }

        /// <summary>
        /// Display cached HTML if available to avoid re-rendering
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Cache the rendered HTML.
            // Tried caching the controls instead to avoid overriding Render, but that creates problems with duplicate ids in the control tree
            if (this.cachedHtml == null)
            {
                // Get the HTML for the page
                var tempWriter = new StringWriter();
                base.Render(new HtmlTextWriter(tempWriter));
                string controlHtml = tempWriter.ToString();

                Context.Cache.Insert(this.cacheKey, controlHtml, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);

                writer.Write(controlHtml);
            }
            else
            {
                base.Render(writer);
            }
        }

        /// <summary>
        /// If checking for tomorrow's closures, look for whether the word "today" has been used and replace it with "tomorrow"
        /// </summary>
        /// <param name="controls"></param>
        private static void UpdateTextForTomorrow(ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                LiteralControl literal = control as LiteralControl;
                if (literal != null) literal.Text = literal.Text.Replace(" today", " tomorrow"); // looking for space hopefully avoids changing URLs
            }
        }
    }
}
