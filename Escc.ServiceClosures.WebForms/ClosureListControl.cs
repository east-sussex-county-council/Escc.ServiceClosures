using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Escc.Dates;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Displays a list of closures as hCalendar events
    /// </summary>
    public class ClosureListControl : WebControl
    {
        private Collection<Service> services = new Collection<Service>();
        private int itemCount;
        private DateTime? onlyDay;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClosureListControl"/> class.
        /// </summary>
        public ClosureListControl()
            : base(HtmlTextWriterTag.Div)
        {
            this.ShowCalendarLink = true;
            this.ShowRssLink = true;
            this.ShowEmailLink = true;

            // default to showing all kinds of closure
            this.ShowEmergencyClosures = true;
            this.ShowPlannedClosures = true;
        }

        /// <summary>
        /// Gets the services containing the closure data.
        /// </summary>
        /// <value>The closures.</value>
        public Collection<Service> Services { get { return this.services; } }

        /// <summary>
        /// Gets or sets a value indicating whether to show only closures in the future.
        /// </summary>
        /// <value><c>true</c> to show future closures only; otherwise, <c>false</c>.</value>
        public bool FutureOnly { get; set; }

        /// <summary>
        /// Gets or sets whether to show only closures for today.
        /// </summary>
        /// <value><c>true</c> tp show closures for today only; otherwise, <c>false</c>.</value>
        public bool TodayOnly
        {
            get
            {
                return (this.onlyDay.HasValue && this.onlyDay.Value == DateTime.Today);
            }
            set
            {
                this.onlyDay = DateTime.Today;

            }
        }

        /// <summary>
        /// Gets or sets whether to show only closures for tomorrow.
        /// </summary>
        /// <value><c>true</c> tp show closures for tomorrow only; otherwise, <c>false</c>.</value>
        public bool TomorrowOnly
        {
            get
            {
                return (this.onlyDay.HasValue && this.onlyDay.Value == DateTime.Today.AddDays(1));
            }
            set
            {
                this.onlyDay = DateTime.Today.AddDays(1);
            }
        }

        /// <summary>
        /// Gets or sets whether to show only closures for a specific day
        /// </summary>
        /// <value>The only day.</value>
        public DateTime? OnlyDay
        {
            get
            {
                return this.onlyDay;
            }
            set
            {
                this.onlyDay = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show emergency closures.
        /// </summary>
        /// <value><c>true</c> to show emergency closures; otherwise, <c>false</c>.</value>
        public bool ShowEmergencyClosures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show closures which are planned in advance and are not emergencies.
        /// </summary>
        /// <value><c>true</c> to show planned closures; otherwise, <c>false</c>.</value>
        public bool ShowPlannedClosures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to group by closure status.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if grouping by closure status; otherwise, <c>false</c>.
        /// </value>
        public bool GroupByClosureStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show a "add these closures to your calendar" link.
        /// </summary>
        /// <value><c>true</c> to show a link; otherwise, <c>false</c>.</value>
        public bool ShowCalendarLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show a "subscribe to closures for this service" RSS link.
        /// </summary>
        /// <value><c>true</c> to show a link; otherwise, <c>false</c>.</value>
        public bool ShowRssLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show a "subscribe to email alerts for this service" link.
        /// </summary>
        /// <value><c>true</c> to show a link; otherwise, <c>false</c>.</value>
        public bool ShowEmailLink { get; set; }

        /// <summary>
        /// Gets or sets the URL of the RSS feed
        /// </summary>
        /// <value>The page URL.</value>
        public Uri RssUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL of the email subscription page
        /// </summary>
        /// <value>The page URL.</value>
        public Uri EmailUrl { get; set; }

        /// <summary>
        /// Gets or sets the page URL to be parsed by microformat requests, if the current page is not to be used
        /// </summary>
        /// <value>The page URL.</value>
        public Uri MicroformatPageUrl { get; set; }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Create a list each for services that are confirmed partly closed, closed or have not provided information.
            // Typical use might be in severe weather, when you might assume a service is closed unless it says it's open.
            if (GroupByClosureStatus)
            {
                string when = String.Empty;
                if (this.TodayOnly) when = " today";
                else if (this.TomorrowOnly) when = " tomorrow";
                else if (this.onlyDay != null) when = " on " + this.onlyDay.Value.ToBritishDateWithDay();

                HtmlGenericControl listPartlyClosed = new HtmlGenericControl("ol");
                listPartlyClosed.Attributes["class"] = this.CssClass;
                AddClosureListItems(listPartlyClosed, ClosureStatus.PartlyClosed);
                if (listPartlyClosed.Controls.Count > 0)
                {
                    this.Controls.Add(new LiteralControl("<h2>Confirmed <strong>partly closed</strong>" + when + "</h2>"));
                    this.Controls.Add(listPartlyClosed);
                }

                HtmlGenericControl listClosed = new HtmlGenericControl("ol");
                listClosed.Attributes["class"] = this.CssClass;
                AddClosureListItems(listClosed, ClosureStatus.Closed);
                if (listClosed.Controls.Count > 0)
                {
                    this.Controls.Add(new LiteralControl("<h2>Confirmed <strong>closed</strong>" + when + "</h2>"));
                    this.Controls.Add(listClosed);
                }
            }
            else
            {
                // Alternatively just display a simple list of all closures.
                // Typical use is all closures for a given service location
                HtmlGenericControl list = new HtmlGenericControl("ol");
                list.Attributes["class"] = this.CssClass;
                AddClosureListItems(list, null);
                if (itemCount > 0) this.Controls.Add(list);
            }

            // Show a link to add closures to calendar, using the hCalendar microformat
            if (itemCount > 0 && this.ShowCalendarLink)
            {
                Uri pageUrl = (this.MicroformatPageUrl == null) ? this.Page.Request.Url : this.MicroformatPageUrl;

                HtmlAnchor hCalendarLink = new HtmlAnchor();
                hCalendarLink.HRef = Path.ChangeExtension(pageUrl.ToString(), ".calendar");
                hCalendarLink.Attributes["data-unpublished"] = "false"; // to avoid warning for CMS editors about unpublished link
                hCalendarLink.InnerText = "Add these closures to your calendar, to be updated automatically";
                hCalendarLink.Attributes["class"] = "hcal subscribe"; // website styles
                this.Controls.Add(hCalendarLink);
            }

            // Show RSS feed link even if there are no items - reader may want to subscribe 
            // even if there are no closures at present, so that they're told when some do come up
            if (this.ShowRssLink && this.RssUrl != null)
            {
                HtmlAnchor rssLink = new HtmlAnchor();
                rssLink.HRef = this.RssUrl.ToString();
                rssLink.InnerText = GetRssSubscribeLinkText();
                rssLink.Attributes["class"] = "rss subscribe";
                rssLink.Attributes["rel"] = "alternate";
                rssLink.Attributes["type"] = "application/rss+xml";
                this.Controls.Add(rssLink);
            }

            // Show email link even if there are no items - reader may want to subscribe 
            // even if there are no closures at present, so that they're told when some do come up
            if (this.ShowEmailLink && this.EmailUrl != null)
            {
                HtmlAnchor emailLink = new HtmlAnchor();
                emailLink.HRef = this.EmailUrl.ToString();
                emailLink.InnerText = GetEmailSubscribeLinkText();
                emailLink.Attributes["class"] = "email subscribe";
                this.Controls.Add(emailLink);
            }
        }

        /// <summary>
        /// Gets the email subscription link text.
        /// </summary>
        /// <returns></returns>
        private string GetEmailSubscribeLinkText()
        {
            if (this.services.Count == 1)
            {
                return "Get email alerts about emergency closures for " + this.services[0].Name;
            }
            else if (this.services.Count > 0)
            {
                return "Get email alerts about emergency " + this.services[0].Type.SingularText + " closures";
            }
            else
            {
                return "Get email alerts about emergency closures";
            }
        }

        /// <summary>
        /// Gets the RSS subscription link text.
        /// </summary>
        /// <returns></returns>
        private string GetRssSubscribeLinkText()
        {
            if (this.services.Count == 1)
            {
                return "Subscribe by RSS to all closures for " + this.services[0].Name;
            }
            else if (this.services.Count > 0)
            {
                return "Subscribe by RSS to all " + this.services[0].Type.SingularText + " closures";
            }
            else
            {
                return "Subscribe by RSS to all closures";
            }
        }

        /// <summary>
        /// Adds the closure list items.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="status">The status.</param>
        private void AddClosureListItems(HtmlGenericControl list, ClosureStatus? status)
        {
            foreach (Service service in this.Services)
            {
                // If a specific date is set, it's going to be on a page designed to show current status of service,
                // rather than a programme of events.
                if (this.onlyDay.HasValue)
                {
                    // Could be multiple closures of one service for the same day. For example, in a school
                    // the infant section could be closed all day and the junior section closed in the afternoon only.
                    Collection<Closure> currentClosures = service.CheckForClosures(this.onlyDay.Value);

                    if (currentClosures.Count > 0)
                    {
                        foreach (Closure closure in currentClosures)
                        {
                            // Beacause the list might be grouped by status, only show if the status
                            // matches (otherwise it'll show multiple times - once for each status)
                            if (status == null || status == closure.Status)
                            {
                                // Only show the closure if the relevant "ShowXXX" property is true
                                if ((ShowEmergencyClosures && closure.Reason.Emergency) ||
                                    (ShowPlannedClosures && !closure.Reason.Emergency))
                                {
                                    AddClosureListItem(service, closure, list);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // If we're not limited to closures on a specific date, we can just show all the closures for the service.
                    foreach (Closure closure in service.Closures)
                    {
                        // ...unless FutureOnly is set, then skip anything in the past
                        if (this.FutureOnly && closure.EndDate < DateTime.Today) continue;

                        // ...and still be careful not to list multiple times when grouping by status
                        if (status == null || status == closure.Status) AddClosureListItem(service, closure, list);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new list item for a closure.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="closure">The closure.</param>
        /// <param name="container">The container.</param>
        private void AddClosureListItem(Service service, Closure closure, Control container)
        {
            HtmlGenericControl listItem = new HtmlGenericControl("li");
            listItem.Attributes["class"] = "vevent"; // hCalendar, but only if there's actual data to add to calendar!
            container.Controls.Add(listItem);

            HtmlGenericControl summary = new HtmlGenericControl("span");
            summary.Attributes["class"] = "summary"; // hCalendar
            listItem.Controls.Add(summary);

            HtmlGenericControl serviceElement = new HtmlGenericControl("span");
            serviceElement.Attributes["class"] = "service"; // in case we need to hide the location and its punctuation together
            summary.Controls.Add(serviceElement);

            HtmlGenericControl location = new HtmlGenericControl("span");
            location.Attributes["class"] = "location"; // hCalendar
            if (service.Url == null)
            {
                location.InnerText = service.Name;
            }
            else
            {
                HtmlAnchor pageLink = new HtmlAnchor();
                pageLink.HRef = service.Url.ToString();
                pageLink.InnerText = service.Name;
                location.Controls.Add(pageLink);
            }
            serviceElement.Controls.Add(location);

            // Display status 
            serviceElement.Controls.Add(new LiteralControl(String.Format(CultureInfo.CurrentCulture, " <span class=\"closed\">{0}</span>, ", Regex.Replace(closure.Status.ToString(), "([A-Z])", " $1").TrimStart().ToLower(CultureInfo.CurrentCulture))));
            summary.Controls.Add(new LiteralControl(closure.Reason.Reason));
            listItem.Controls.Add(new LiteralControl(": "));

            if (closure.StartDate == closure.EndDate)
            {
                HtmlGenericControl dtstart = new HtmlGenericControl("time");
                dtstart.Attributes["class"] = "dtstart"; // hCalendar
                dtstart.Attributes["datetime"] = closure.StartDate.ToIso8601Date();
                dtstart.InnerText = closure.StartDate.ToBritishDateWithDay();
                dtstart.InnerText = dtstart.InnerText.Remove(dtstart.InnerText.Length - 4); // Remove the year, because we put that inside a separate tag
                listItem.Controls.Add(dtstart);

                // Date in datetime attribute here should be one day later. 
                // For example, if closure is for 15 May, should say 16 May.
                // It's referring to 00:00 hours at the start of 16 May, meaning the end of 15 May.
                HtmlGenericControl dtend = new HtmlGenericControl("time");
                dtend.Attributes["class"] = "dtend"; // hCalendar
                dtend.Attributes["datetime"] = closure.StartDate.AddDays(1).ToIso8601Date();
                dtend.InnerText = closure.StartDate.Year.ToString(CultureInfo.CurrentCulture);
                listItem.Controls.Add(dtend);
            }
            else
            {
                HtmlGenericControl dtstart = new HtmlGenericControl("time");
                dtstart.Attributes["class"] = "dtstart dtstamp"; // hCalendar. Dtstamp doesn't need to be accurate, just needs to be present for Outlook 2003 to work.
                dtstart.Attributes["datetime"] = closure.StartDate.ToIso8601Date();
                dtstart.InnerText = closure.StartDate.ToBritishDateWithDay();
                listItem.Controls.Add(dtstart);

                listItem.Controls.Add(new LiteralControl(" to "));

                // Date in datetime attribute here should be one day later. 
                // For example, if closure is for 15 May, should say 16 May.
                // It's referring to 00:00 hours at the start of 16 May, meaning the end of 15 May.
                HtmlGenericControl dtend = new HtmlGenericControl("time");
                dtend.Attributes["class"] = "dtend"; // hCalendar
                dtend.Attributes["datetime"] = closure.EndDate.AddDays(1).ToIso8601Date();
                dtend.InnerText = closure.EndDate.ToBritishDateWithDay();
                listItem.Controls.Add(dtend);
            }

            if (!String.IsNullOrEmpty(closure.Notes))
            {
                listItem.Controls.Add(new LiteralControl("<p>" + HttpUtility.HtmlEncode(closure.Notes).Replace(Environment.NewLine + Environment.NewLine, "</p><p>") + "</p>"));
            }

            // UID is for hCalendar, and particularly for Outlook 2003 which won't import events without it
            listItem.Controls.Add(new LiteralControl("<span class=\"uid\">http://www.eastsussex.gov.uk/id/" + service.Type.SingularText.ToLowerInvariant() + "/" + service.Code + "/closure/" + closure.Id + "</span>"));

            itemCount++;
        }

        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            // Don't render the begin tag - the child controls will handle it    
        }

        /// <summary>
        /// Renders the HTML closing tag of the control into the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            // Don't render the end tag - the child controls will handle it
        }
    }
}
