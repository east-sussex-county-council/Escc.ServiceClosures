using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.IO;
using System.Web.UI.HtmlControls;
using System.ComponentModel;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Container control which displays only if one of its child <see cref="EmergencyClosuresLink"/> controls is also displayed
    /// </summary>
    /// <example>
    /// <para>To present links to closure pages which only appear when emergency closures are in effect:</para>
    /// <code>
    /// &lt;%@ Register TagPrefix=&quot;ServiceClosures&quot; Assembly=&quot;Escc.ServiceClosures, Version=1.0.0.0, Culture=neutral, PublicKeyToken=06fad7304560ae6f&quot; Namespace=&quot;Escc.ServiceClosures&quot; %&gt;
    ///
    /// &lt;ServiceClosures:EmergencyClosuresLinkContainer runat=&quot;server&quot;&gt;
    /// &lt;ContentTemplate&gt;
    ///     &lt;h2&gt;Services closed&lt;/h2&gt;
    ///     &lt;ul&gt;
    ///
    ///     &lt;ServiceClosures:EmergencyClosuresLink runat=&quot;server&quot; ServiceType=&quot;School&quot; NavigateUrl=&quot;http://www.example.com&quot; Text=&quot;School closures&quot; CssClass=&quot;schoolLink&quot;&gt;
    ///         &lt;HeaderTemplate&gt;&lt;li&gt;&lt;/HeaderTemplate&gt;
    ///         &lt;FooterTemplate&gt;&lt;/li&gt;&lt;/FooterTemplate&gt;
    ///     &lt;/ServiceClosures:EmergencyClosuresLink&gt;
    ///
    ///     &lt;ServiceClosures:EmergencyClosuresLink runat=&quot;server&quot; ServiceType=&quot;Library&quot; NavigateUrl=&quot;http://www.example.com&quot; Text=&quot;Library closures&quot; CssClass=&quot;libraryLink&quot;&gt;
    ///         &lt;HeaderTemplate&gt;&lt;li&gt;&lt;/HeaderTemplate&gt;
    ///         &lt;FooterTemplate&gt;&lt;/li&gt;&lt;/FooterTemplate&gt;
    ///     &lt;/ServiceClosures:EmergencyClosuresLink&gt;
    ///
    ///     &lt;/ul&gt;
    /// &lt;/ContentTemplate&gt;
    /// &lt;/ServiceClosures:EmergencyClosuresLinkContainer&gt;
    /// </code>
    /// </example>
    public class EmergencyClosuresLinkContainer : WebControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmergencyClosuresLinkContainer"/> class.
        /// </summary>
        public EmergencyClosuresLinkContainer() : base(HtmlTextWriterTag.Div) { }

        /// <summary>
        /// Gets or sets the child controls, which should include at least one <see cref="EmergencyClosuresLink"/>
        /// </summary>
        /// <value>The template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio (allegedly)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio (allegedly)
        public ITemplate ContentTemplate { get; set; }

        /// <summary>
        /// Container for header and footer templates
        /// </summary>
        private class XhtmlContainer : PlaceHolder, INamingContainer { }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            bool childControlVisible = false;

            if (ContentTemplate != null)
            {
                XhtmlContainer content = new XhtmlContainer();
                ContentTemplate.InstantiateIn(content);
                this.Controls.Add(content);

                foreach (Control control in content.Controls)
                {
                    if (control.GetType() == typeof(EmergencyClosuresLink) && control.Visible)
                    {
                        childControlVisible = true;
                        break;
                    }
                }
            }

            this.Visible = childControlVisible;
        }
    }
}
